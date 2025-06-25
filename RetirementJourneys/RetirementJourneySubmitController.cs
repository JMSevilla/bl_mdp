using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.IdentityVerification.Services;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.Db;
using WTW.MdpService.Infrastructure.EmailConfirmation;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.MdpService.Infrastructure.Templates.RetirementApplication.Submit;
using WTW.MdpService.Journeys.Submit.Services;
using WTW.MdpService.Retirement;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;
using static WTW.Web.MdpConstants;

namespace WTW.MdpService.RetirementJourneys;

[ApiController]
public class RetirementJourneySubmitController : ControllerBase
{
    private readonly IRetirementJourneyRepository _repository;
    private readonly IEmailConfirmationSmtpClient _emailConfirmationSmtpClient;
    private readonly IMemberRepository _memberRepository;
    private readonly IMemberDbUnitOfWork _memberDbUnitOfWork;
    private readonly IMdpUnitOfWork _mdpUnitOfWork;
    private readonly ILogger<RetirementJourneySubmitController> _logger;
    private readonly IContentClient _contentClient;
    private readonly ICalculationsParser _calculationsParser;
    private readonly IRetirementApplicationSubmissionTemplate _retirementApplicationSubmissionTemplate;
    private readonly IDocumentsRendererDataFactory _documentsRendererDataFactory;
    private readonly IDocumentRenderer _documentRenderer;
    private readonly ICaseRequestFactory _caseApiRequestModelFactory;
    private readonly ICaseService _caseService;
    private readonly IDocumentsUploaderService _documentUploader;
    private readonly IIdvService _idvService;

    public RetirementJourneySubmitController(
        IRetirementJourneyRepository repository,
        IEmailConfirmationSmtpClient emailConfirmationSmtpClient,
        IMemberRepository memberRepository,
        IMemberDbUnitOfWork memberDbUnitOfWork,
        IMdpUnitOfWork mdpUnitOfWork,
        IContentClient contentClient,
        ILogger<RetirementJourneySubmitController> logger,
        ICalculationsParser calculationsParser,
        IRetirementApplicationSubmissionTemplate retirementApplicationSubmissionTemplate,
        IDocumentsRendererDataFactory documentsRendererDataFactory,
        IDocumentRenderer documentRenderer,
        ICaseRequestFactory caseApiRequestModelFactory,
        ICaseService caseService,
        IDocumentsUploaderService documentUploader,
        IIdvService idvService)
    {
        _repository = repository;
        _emailConfirmationSmtpClient = emailConfirmationSmtpClient;
        _memberRepository = memberRepository;
        _memberDbUnitOfWork = memberDbUnitOfWork;
        _mdpUnitOfWork = mdpUnitOfWork;
        _contentClient = contentClient;
        _logger = logger;
        _calculationsParser = calculationsParser;
        _retirementApplicationSubmissionTemplate = retirementApplicationSubmissionTemplate;
        _documentsRendererDataFactory = documentsRendererDataFactory;
        _documentRenderer = documentRenderer;
        _caseApiRequestModelFactory = caseApiRequestModelFactory;
        _caseService = caseService;
        _documentUploader = documentUploader;
        _idvService = idvService;
    }

    [HttpPut]
    [Route("api/v2/retirement-journey/submit")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> SubmitV2([FromBody] SubmitRetirementJourneyV2Request request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var now = DateTimeOffset.UtcNow;
        var journeyType = "dbretirementapplication";
        return await _repository
            .FindUnexpiredUnsubmittedJourney(businessGroup, referenceNumber, now)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    //Todo: Came back and fix posible null exception after FE migration to this endpoint                   
                    if (IsAcknowledgementRequired(request, journey))
                        return BadRequest(ApiError.FromMessage("Acknowledgement must be confirmed."));

                    var member = (await _memberRepository.FindMember(referenceNumber, businessGroup)).SingleOrDefault();
                    if (member == null)
                        return BadRequest(ApiError.FromMessage("Member was not found."));

                    journey.SetFlags(request.AcknowledgementFinancialAdvisor, request.AcknowledgementPensionWise);
                    await _mdpUnitOfWork.Commit();

                    var multiDbTransaction = await new MultiDbTransaction(_memberDbUnitOfWork, _mdpUnitOfWork).Begin();

                    var requestModel = _caseApiRequestModelFactory.CreateForGenericRetirement(businessGroup, referenceNumber);
                    var caseResult = await _caseService.Create(requestModel);
                    if (caseResult.IsLeft)
                    {
                        _logger.LogError("Failed to create submit case. Retirement journey. Error: {error}", caseResult.Left().Message);
                        return BadRequest(new ApiError("Failed to create case."));
                    }

                    var errorOrIdvSaved = await _idvService.SaveIdentityVerification(businessGroup,
                        referenceNumber, CaseCodes.RTP9, caseResult.Right());
                    if (errorOrIdvSaved.IsLeft)
                    {
                        _logger.LogWarning("Failed to save IDV results.Error: {error}", errorOrIdvSaved.Left().Message);
                    }

                    var submissionEmailTemplate = await _contentClient.FindTemplate("retirement_application_submission_email", request.ContentAccessKey, $"{member.SchemeCode}-{member.Category}");

                    var documentsRendererData = _documentsRendererDataFactory.CreateForSubmit(journeyType, businessGroup, referenceNumber, request.ContentAccessKey, caseResult.Right());
                    var document = await _documentRenderer.RenderGenericSummaryPdf(
                        documentsRendererData,
                        Request.Headers[HeaderNames.Authorization],
                        Request.Headers["env"]);

                    var errorOrImageId = await _documentUploader.UploadDBRetirementDocument(businessGroup,
                                                                                            referenceNumber,
                                                                                            caseResult.Right(),
                                                                                            document.PdfStream,
                                                                                            document.FileName,
                                                                                            journey.GbgId);
                    if (errorOrImageId.IsLeft)
                    {
                        _logger.LogError("Failed to upload retirement submit documents.Error: {error}", errorOrImageId.Left().Message);
                        return BadRequest(ApiError.FromMessage(errorOrImageId.Left().Message));
                    }

                    try
                    {
                        journey.Submit(
                            document.PdfStream.ToArray(),
                            now,
                            caseResult.Right());

                        var email = member.Email().SingleOrDefault();

                        var emailTemplate = await _retirementApplicationSubmissionTemplate.Render(
                            submissionEmailTemplate.HtmlBody,
                            request.ContentAccessKey,
                            new CmsTokenInformationResponse(),
                            journey,
                            member,
                            submissionEmailTemplate.ContentBlockKeys);

                        var emailSubject = await _retirementApplicationSubmissionTemplate.Render(
                            submissionEmailTemplate.EmailSubject,
                            request.ContentAccessKey,
                            new CmsTokenInformationResponse(),
                            journey,
                            member,
                            submissionEmailTemplate.ContentBlockKeys);

                        await multiDbTransaction.Commit();
                        if (!string.IsNullOrEmpty(email))
                        {
                            try
                            {
                                await _emailConfirmationSmtpClient.Send(
                                    email,
                                    submissionEmailTemplate.EmailFrom,
                                    emailTemplate,
                                    emailSubject);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to send email");
                            }
                        }

                        return NoContent();
                    }
                    catch
                    {
                        await multiDbTransaction.Rollback();
                        throw;
                    }
                },
                () => NotFound(ApiError.NotFound()));
    }

    private bool IsAcknowledgementRequired(SubmitRetirementJourneyV2Request request, RetirementJourney journey)
    {
        var calculation = _calculationsParser.GetRetirementV2(journey.Calculation.RetirementJsonV2);
        return calculation.HasAdditionalContributions() && !request.Acknowledgement;
    }

    //Temp endpoint for testing options 2.0 pdf
    [HttpGet("api/v2/retirement-journey/download/pdf")]
    [ProducesResponseType(typeof(FileResult), 200, "application/octet-stream")]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GeneratePdf([Required][FromQuery] string contentAccessKey)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var now = DateTimeOffset.UtcNow;

        return await _repository
            .FindUnexpiredUnsubmittedJourney(businessGroup, referenceNumber, now)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    var member = (await _memberRepository.FindMember(referenceNumber, businessGroup)).SingleOrDefault();
                    if (member == null)
                        return BadRequest(ApiError.FromMessage("Member was not found."));

                    var summaryPdf = await GenerateSummaryPdf(contentAccessKey, businessGroup, referenceNumber, string.Empty);

                    return File(summaryPdf.ToArray(), "application/pdf", "retirement-journey-test-for-options2.pdf");
                },
                () => NotFound(ApiError.NotFound()));
    }

    private async Task<MemoryStream> GenerateSummaryPdf(
       string contentAccessKey,
       string businessGroup,
       string referenceNumber,
       string caseNumber)
    {
        var documentsRendererData = _documentsRendererDataFactory.CreateForSubmit("dbretirementapplication", businessGroup, referenceNumber, contentAccessKey, caseNumber);

        var document = await _documentRenderer.RenderGenericSummaryPdf(
            documentsRendererData,
            Request.Headers[HeaderNames.Authorization],
            Request.Headers["env"]);

        return document.PdfStream;
    }
}