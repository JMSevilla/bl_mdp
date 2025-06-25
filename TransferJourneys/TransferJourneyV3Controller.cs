using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.IdentityVerification.Services;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.Edms;
using WTW.MdpService.Infrastructure.EdmsIndexing;
using WTW.MdpService.Infrastructure.EmailConfirmation;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.MdpService.Infrastructure.MemberDb.Documents;
using WTW.MdpService.Infrastructure.PdfGenerator;
using WTW.MdpService.Infrastructure.Templates.TransferApplication;
using WTW.Web.Authorization;
using WTW.Web;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;
using WTW.Web.Templates;
using static WTW.Web.MdpConstants;

namespace WTW.MdpService.TransferJourneys;

[ApiController]
[Route("api/v3/transfer-journeys")]
public class TransferJourneyV3Controller : ControllerBase
{
    private const string SubmitPdfTemplateKey = "transfer_v2_pdf";
    private const string SubmitEmailTemplateKey = "transfer_v2_application_submission_email";
    private const string SubmitPdfFileName = "Transfer_application_summary.pdf";

    private readonly ITransferCalculationRepository _transferCalculationRepository;
    private readonly IEdmsClient _edmsClient;
    private readonly IMemberDbUnitOfWork _uow;
    private readonly ITransferJourneyRepository _transferJourneyRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IMdpUnitOfWork _mdpDbUnitOfWork;
    private readonly IContentClient _contentClient;
    private readonly IEmailConfirmationSmtpClient _smtpClient;
    private readonly ITransferCase _transferCase;
    private readonly IEdmsDocumentsIndexing _edmsDocumentsIndexing;
    private readonly ITransferV2Template _transferV2Template;
    private readonly ITransferJourneySubmitEmailTemplate _transferJourneySubmitEmailTemplate;
    private readonly IJourneyDocumentsRepository _journeyDocumentsRepository;
    private readonly IDocumentsRepository _documentsRepository;
    private readonly ILogger<TransferJourneyV3Controller> _logger;
    private readonly IDocumentFactoryProvider _documentFactoryProvider;
    private readonly IPdfGenerator _pdfGenerator;
    private readonly ICalculationsRepository _calculationsRepository;
    private readonly ICalculationsParser _calculationsParser;
    private readonly IIdvService _idvService;

    public TransferJourneyV3Controller(
        ITransferCalculationRepository transferCalculationRepository,
        IEdmsClient edmsClient,
        IMemberDbUnitOfWork uow,
        ITransferJourneyRepository transferJourneyRepository,
        IMdpUnitOfWork mdpDbUnitOfWork,
        IContentClient contentClient,
        IMemberRepository memberRepository,
        IEmailConfirmationSmtpClient smtpClient,
        ITransferCase transferCase,
        IEdmsDocumentsIndexing edmsDocumentsIndexing,
        ITransferV2Template transferV2Template,
        ITransferJourneySubmitEmailTemplate transferJourneySubmitEmailTemplate,
        IJourneyDocumentsRepository journeyDocumentsRepository,
        IDocumentsRepository documentsRepository,
        ILogger<TransferJourneyV3Controller> logger,
        IDocumentFactoryProvider documentFactoryProvider,
        IPdfGenerator pdfGenerator,
        ICalculationsRepository calculationsRepository,
        ICalculationsParser calculationsParser,
        IIdvService idvService)
    {
        _transferCalculationRepository = transferCalculationRepository;
        _edmsClient = edmsClient;
        _uow = uow;
        _transferJourneyRepository = transferJourneyRepository;
        _memberRepository = memberRepository;
        _mdpDbUnitOfWork = mdpDbUnitOfWork;
        _contentClient = contentClient;
        _smtpClient = smtpClient;
        _transferCase = transferCase;
        _edmsDocumentsIndexing = edmsDocumentsIndexing;
        _transferV2Template = transferV2Template;
        _transferJourneySubmitEmailTemplate = transferJourneySubmitEmailTemplate;
        _journeyDocumentsRepository = journeyDocumentsRepository;
        _documentsRepository = documentsRepository;
        _logger = logger;
        _documentFactoryProvider = documentFactoryProvider;
        _pdfGenerator = pdfGenerator;
        _calculationsRepository = calculationsRepository;
        _calculationsParser = calculationsParser;
        _idvService = idvService;
    }

    [HttpPost("submit")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> Submit([FromBody] TransferJourneySubmitRequest request)
    {
        var now = DateTimeOffset.UtcNow;
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        return await _transferJourneyRepository.Find(businessGroup, referenceNumber)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    var member = await _memberRepository.FindMember(referenceNumber, businessGroup);
                    if (member.IsNone)
                        return BadRequest(ApiError.FromMessage("Member was not found."));

                    var transferCalculation = (await _transferCalculationRepository.Find(businessGroup, referenceNumber)).Value();
                    transferCalculation.SetStatus(TransferApplicationStatus.SubmittedTA);
                    var transferQuote = _calculationsParser.GetTransferQuote(transferCalculation.TransferQuoteJson);

                    var errorOrCaseNumber = await _transferCase.Create(businessGroup, referenceNumber);
                    if (errorOrCaseNumber.IsLeft)
                    {
                        _logger.LogError("Failed to create transfer case: Error: {message}", errorOrCaseNumber.Left().Message);
                        return BadRequest(ApiError.FromMessage("Failed to create transfer case."));
                    }
                    journey.Submit(errorOrCaseNumber.Right(), now);

                    var errorOrIdvSaved = await _idvService.SaveIdentityVerification(businessGroup, referenceNumber, CaseCodes.TOP9, errorOrCaseNumber.Right());
                    if (errorOrIdvSaved.IsLeft)
                    {
                        _logger.LogWarning("Failed to save IDV results.Error: {error}", errorOrIdvSaved.Left().Message);
                    }

                    var documents = await _journeyDocumentsRepository.List(businessGroup, referenceNumber, new List<string> { MdpConstants.JourneyTypeTransferV2, MdpConstants.IdentityDocumentType });
                    var retirementCalculation = await _calculationsRepository.Find(referenceNumber, businessGroup);
                    var pdfStream = await RenderSubmitPdfSummary(journey, member.Value(), transferQuote, transferCalculation, request.ContentAccessKey, documents, retirementCalculation);

                    var edmsResult = await _edmsClient.UploadDocument(
                        businessGroup,
                        SubmitPdfFileName,
                        pdfStream);
                    if (edmsResult.IsLeft)
                    {
                        _logger.LogWarning("Failed to upload \"{pdf}\". Error: {message}", SubmitPdfFileName, edmsResult.Left().Message);
                        return BadRequest(ApiError.FromMessage(edmsResult.Left().Message));
                    }

                    var hasAvc = retirementCalculation.MatchUnsafe(r => !string.IsNullOrEmpty(r.RetirementJsonV2) ? _calculationsParser.GetRetirementV2(r.RetirementJsonV2).HasAdditionalContributions() : false, () => false);
                    var document = new UploadedDocument(referenceNumber, businessGroup, "transfer2", null, SubmitPdfFileName, edmsResult.Right().Uuid, DocumentSource.Outgoing, true, "TRNAPP", "TVOUT", hasAvc ? "PWGFR" : null);
                    var allDocumentsCopy = documents.ToList();
                    allDocumentsCopy.Add(document);
                    var errorOrDocuments = await _edmsDocumentsIndexing.PostIndexTransferDocuments(businessGroup, referenceNumber, errorOrCaseNumber.Right(), journey, allDocumentsCopy);
                    if (errorOrDocuments.IsLeft)
                    {
                        _logger.LogWarning("Failed to postindex documents. Error: {message}", errorOrDocuments.Left().Message);
                        return BadRequest(ApiError.FromMessage(errorOrDocuments.Left().Message));
                    }

                    var summaryImageId = await CreateNewMemberDocument(now, referenceNumber, businessGroup, errorOrCaseNumber.Right(), edmsResult.Right(), errorOrDocuments.Right());
                    journey.SaveTransferSummaryImageId(summaryImageId);
                    _logger.LogInformation("Generated transfer summary image id is:{imageId}", summaryImageId);

                    _journeyDocumentsRepository.RemoveAll(documents);
                    await _mdpDbUnitOfWork.Commit();
                    await _uow.Commit();

                    await SendSubmitEmail(member.Value(), errorOrCaseNumber.Right(), request.ContentAccessKey);
                    return NoContent();
                },
                () => NotFound(ApiError.NotFound()));
    }

    private async Task<MemoryStream> RenderSubmitPdfSummary(
       TransferJourney journey,
       Member member,
       TransferQuote transferQuote,
       TransferCalculation transferCalculation,
       string contentAccessKey,
       IEnumerable<UploadedDocument> documents,
       Option<Calculation> retirementCalculation)
    {
        var pdfTemplate = await _contentClient.FindTemplate(SubmitPdfTemplateKey, contentAccessKey, $"{member.SchemeCode}-{member.Category}");
        var renderedTransferHtml = await _transferV2Template.RenderHtml(
            pdfTemplate.HtmlBody,
            journey,
            member,
            transferQuote,
            member.GetTransferApplicationStatus(transferCalculation),
            DateTimeOffset.UtcNow,
            documents,
            retirementCalculation);
        return await _pdfGenerator.Generate(renderedTransferHtml, pdfTemplate.HtmlHeader, pdfTemplate.HtmlFooter);
    }

    private async Task<int> CreateNewMemberDocument(DateTimeOffset now, string referenceNumber, string businessGroup, string caseNumber, DocumentUploadResponse edmsResult, List<(int ImageId, string DocUuid)> documents)
    {
        var summaryImageId = documents.Single(x => x.DocUuid == edmsResult.Uuid).ImageId;
        var memberDocument = _documentFactoryProvider.GetFactory(DocumentType.TransferV2).Create(
                                businessGroup,
                                referenceNumber,
                                await _documentsRepository.NextId(),
                                summaryImageId,
                                now,
                                caseNumber);
        _documentsRepository.Add(memberDocument);
        return summaryImageId;
    }

    private async Task SendSubmitEmail(Member member, string caseNumber, string contentAccessKey)
    {
        var renderedEmail = await RenderSubmitEmail(member, caseNumber, contentAccessKey);
        try
        {
            await _smtpClient.Send(
                member.Email().SingleOrDefault(),
                renderedEmail.EmailFrom,
                renderedEmail.EmailHtmlBody,
                renderedEmail.EmailSubject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send transfer submit email");
        }
    }

    private async Task<(string EmailHtmlBody, string EmailFrom, string EmailSubject)> RenderSubmitEmail(Member member, string caseNumber, string contentAccessKey)
    {
        var emailTemplate = await _contentClient.FindTemplate(SubmitEmailTemplateKey, contentAccessKey, $"{member.SchemeCode}-{member.Category}");
        var emailHtmlBody = new Template(emailTemplate.HtmlBody).Apply(new Dictionary<string, string>()
        {
            ["token:transfer_reference_number"] = caseNumber
        });

        return (await _transferJourneySubmitEmailTemplate.RenderHtml(emailHtmlBody, member), emailTemplate.EmailFrom, emailTemplate.EmailSubject);
    }
}