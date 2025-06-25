using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using WTW.MdpService.Domain;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Aws;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.Edms;
using WTW.MdpService.Infrastructure.EdmsIndexing;
using WTW.MdpService.Infrastructure.EmailConfirmation;
using WTW.MdpService.Infrastructure.Journeys.Documents;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.MdpService.Infrastructure.MemberDb.Documents;
using WTW.MdpService.Infrastructure.PdfGenerator;
using WTW.MdpService.Infrastructure.Redis;
using WTW.MdpService.Infrastructure.Templates.TransferApplication;
using WTW.MdpService.Templates;
using WTW.Web;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.Extensions;
using WTW.Web.LanguageExt;
using WTW.Web.Templates;

namespace WTW.MdpService.TransferJourneys;

[ApiController]
[Route("api/v2/transfer-journeys")]
public class TransferJourneyV2Controller : ControllerBase
{
    private const string SubmitPdfTemplateKey = "transfer_v2_pdf";
    private const string SubmitEmailTemplateKey = "transfer_v2_application_submission_email";
    private const string SubmitPdfFileName = "Transfer_application_summary.pdf";

    private readonly ICalculationsClient _calculationsClient;
    private readonly ITransferCalculationRepository _transferCalculationRepository;
    private readonly ICalculationsRedisCache _calculationsRedisCache;
    private readonly IAwsClient _awsClient;
    private readonly IEdmsClient _edmsClient;
    private readonly ICalculationHistoryRepository _calculationHistoryRepository;
    private readonly IRetirementPostIndexEventRepository _postIndexEventsRepository;
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
    private readonly ILogger<TransferJourneyV2Controller> _logger;
    private readonly IDocumentFactoryProvider _documentFactoryProvider;
    private readonly ITransferJourneyContactFactory _transferJourneyContactFactory;
    private readonly IPdfGenerator _pdfGenerator;
    private readonly ITemplateService _templateService;
    private readonly ICalculationsRepository _calculationsRepository;
    private readonly ICalculationsParser _calculationsParser;
    private readonly IJourneyDocumentsHandlerService _journeyDocumentsHandlerService;

    public TransferJourneyV2Controller(
        ICalculationsClient calculationsClient,
        ITransferCalculationRepository transferCalculationRepository,
        ICalculationsRedisCache calculationsRedisCache,
        IAwsClient awsClient,
        IEdmsClient edmsClient,
        ICalculationHistoryRepository calculationHistoryRepository,
        IRetirementPostIndexEventRepository postIndexEventsRepository,
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
        ILogger<TransferJourneyV2Controller> logger,
        IDocumentFactoryProvider documentFactoryProvider,
        ITransferJourneyContactFactory transferJourneyContactFactory,
        IPdfGenerator pdfGenerator,
        ITemplateService templateService,
        ICalculationsRepository calculationsRepository,
        ICalculationsParser calculationsParser,
        IJourneyDocumentsHandlerService journeyDocumentsHandlerService)
    {
        _calculationsClient = calculationsClient;
        _transferCalculationRepository = transferCalculationRepository;
        _calculationsRedisCache = calculationsRedisCache;
        _awsClient = awsClient;
        _edmsClient = edmsClient;
        _calculationHistoryRepository = calculationHistoryRepository;
        _postIndexEventsRepository = postIndexEventsRepository;
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
        _transferJourneyContactFactory = transferJourneyContactFactory;
        _pdfGenerator = pdfGenerator;
        _templateService = templateService;
        _calculationsRepository = calculationsRepository;
        _calculationsParser = calculationsParser;
        _journeyDocumentsHandlerService = journeyDocumentsHandlerService;
    }

    [HttpPost("start")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> StartTransferJourney([FromBody] StartTransferJourneyRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        var result = await _transferJourneyRepository.Find(businessGroup, referenceNumber);
        if (result.IsSome)
            _transferJourneyRepository.Remove(result.Value());

        var hardQuoteResult = await _calculationsClient.HardQuote(businessGroup, referenceNumber);
        if (hardQuoteResult.IsLeft)
        {
            _logger.LogWarning("Failed to lock transfer quote. Error: {message}", hardQuoteResult.Left().Message);
            return BadRequest(ApiError.FromMessage(hardQuoteResult.Left().Message));
        }

        var transferCalculation = await _transferCalculationRepository.Find(businessGroup, referenceNumber);
        transferCalculation.Value().LockTransferQoute();
        var templates = await _templateService.DownloadTemplates(request.ContentAccessKey);
        _logger.LogInformation("Templates count: {count}", templates.Count);
        var pdfresult = await _pdfGenerator.GeneratePages(templates);
        await _calculationsRedisCache.ClearRetirementDateAges(referenceNumber, businessGroup);

        var awsResult = await _awsClient.File(hardQuoteResult.Right());
        if (awsResult.IsLeft)
            return BadRequest(ApiError.FromMessage(awsResult.Left().Message));

        var finalData = await _pdfGenerator.MergePdfs(awsResult.Right().ToArray(), pdfresult);
        using var finalStream = new MemoryStream(finalData);

        if (finalStream == null || finalStream.Length == 0)
        {
            string error = finalStream == null ? "The stream is null." : "The stream is empty.";
            _logger.LogWarning("Failed to generate merged PDF. {Error}", error);
            return BadRequest(ApiError.FromMessage($"Failed to generate PDF. {error}"));
        }

        var edmsResult = await _edmsClient.PreindexDocument(
            businessGroup,
            referenceNumber,
            $"{businessGroup}1",
            finalStream);
        if (edmsResult.IsLeft)
        {
            transferCalculation.Value().UnlockTransferQoute();
            await _mdpDbUnitOfWork.Commit();
            _logger.LogWarning("EDMS preIndex failed: {message}", edmsResult.Left().Message);
            return BadRequest(ApiError.FromMessage($"EDMS preIndex failed: {edmsResult.Left().Message}"));
        }

        var typeResponse = (await _calculationsClient.TransferEventType(businessGroup, referenceNumber).Try()).Value();

        _logger.LogInformation("Calling CalcApi to get LockedInTransferQuoteSeqno with business group:{businessGroup} and refno:{referenceNumber}", businessGroup, referenceNumber);
        var datesAgesResponse = await _calculationsClient.RetirementDatesAges(referenceNumber, businessGroup).Try();
        if (!datesAgesResponse.IsSuccess)
        {
            _logger.LogError("Failed to retrieve LockedInTransferQuoteSeqno value from calc api. Business group:{businessGroup}, Refno:{referenceNumber}.", businessGroup, referenceNumber);
            return BadRequest(ApiError.FromMessage("Failed to receive transfer quote information"));
        }

        if (datesAgesResponse.Value().LockedInTransferQuoteSeqno == null)
        {
            _logger.LogError("LockedInTransferQuoteSeqno value is null. Business group:{businessGroup}, Refno:{referenceNumber}", businessGroup, referenceNumber);
            return BadRequest(ApiError.FromMessage("Seqno does not have value"));
        }

        var calculationHistory = await _calculationHistoryRepository.FindByEventTypeAndSeqNumber(businessGroup, referenceNumber, typeResponse.Type, datesAgesResponse.Value().LockedInTransferQuoteSeqno.Value);

        if (calculationHistory.IsNone)
        {
            transferCalculation.Value().UnlockTransferQoute();
            await _mdpDbUnitOfWork.Commit();
            _logger.LogWarning("Calc history record cannnot be found");
            return BadRequest(ApiError.FromMessage("Calc history record cannnot be found"));
        }

        calculationHistory.Value().UpdateIds(edmsResult.Right().ImageId, edmsResult.Right().BatchNumber);

        _postIndexEventsRepository.Add(
                new RetirementPostIndexEvent(
                    businessGroup,
                    referenceNumber,
                    "TRANSFER",
                    edmsResult.Right().BatchNumber,
                    edmsResult.Right().ImageId));

        var journey = TransferJourney.Create(
            businessGroup,
            referenceNumber,
            DateTimeOffset.UtcNow,
            request.CurrentPageKey,
            request.NextPageKey,
            edmsResult.Right().ImageId);
        await _transferJourneyRepository.Create(journey);
        await _mdpDbUnitOfWork.Commit();
        await _uow.Commit();
        return NoContent();
    }

    [HttpGet("download-guaranteed-transfer")]
    [ProducesResponseType(typeof(FileResult), 200, "application/octet-stream")]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> DownloadGuaranteedTransferQuote([FromQuery] string contentAccessKey)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        var result = await _transferJourneyRepository.Find(businessGroup, referenceNumber);
        if (result.IsSome)
            _transferJourneyRepository.Remove(result.Value());

        var transferQuoteResult = await _calculationsClient.GetGuaranteedTransfer(businessGroup, referenceNumber);
        if (transferQuoteResult.IsLeft)
        {
            _logger.LogWarning("Failed to lock transfer quote. Error: {message}", transferQuoteResult.Left().Message);
            return BadRequest(ApiError.FromMessage(transferQuoteResult.Left().Message));
        }

        var transferCalculation = await _transferCalculationRepository.Find(businessGroup, referenceNumber);
        transferCalculation.Value().LockTransferQoute();
        var templates = await _templateService.DownloadTemplates(contentAccessKey);
        _logger.LogInformation("Templates count: {count}", templates.Count);
        var pdfresult = await _pdfGenerator.GeneratePages(templates);
        await _calculationsRedisCache.ClearRetirementDateAges(referenceNumber, businessGroup);

        var awsResult = await _awsClient.File(transferQuoteResult.Right());
        if (awsResult.IsLeft)
            return BadRequest(ApiError.FromMessage(awsResult.Left().Message));

        var finalData = await _pdfGenerator.MergePdfs(awsResult.Right().ToArray(), pdfresult);
        using var finalStream = new MemoryStream(finalData);

        if (finalStream.Length == 0)
        {
            string error = "The stream is empty.";
            _logger.LogWarning("Failed to generate merged PDF. {Error}", error);
            return BadRequest(ApiError.FromMessage($"Failed to generate PDF. {error}"));
        }

        var edmsResult = await _edmsClient.PreindexDocument(
            businessGroup,
            referenceNumber,
            $"{businessGroup}1",
            finalStream);
        if (edmsResult.IsLeft)
        {
            transferCalculation.Value().UnlockTransferQoute();
            await _mdpDbUnitOfWork.Commit();
            _logger.LogWarning("EDMS preIndex failed: {message}", edmsResult.Left().Message);
            return BadRequest(ApiError.FromMessage($"EDMS preIndex failed: {edmsResult.Left().Message}"));
        }

        var typeResponse = (await _calculationsClient.TransferEventType(businessGroup, referenceNumber).Try()).Value();

        _logger.LogInformation("Calling CalcApi to get LockedInTransferQuoteSeqno with business group:{businessGroup} and refno:{referenceNumber}", businessGroup, referenceNumber);
        var datesAgesResponse = await _calculationsClient.RetirementDatesAges(referenceNumber, businessGroup).Try();
        if (!datesAgesResponse.IsSuccess)
        {
            _logger.LogError("Failed to retrieve LockedInTransferQuoteSeqno value from calc api. Business group:{businessGroup}, Refno:{referenceNumber}.", businessGroup, referenceNumber);
            return BadRequest(ApiError.FromMessage("Failed to receive transfer quote information"));
        }

        if (datesAgesResponse.Value().LockedInTransferQuoteSeqno == null)
        {
            _logger.LogError("LockedInTransferQuoteSeqno value is null. Business group:{businessGroup}, Refno:{referenceNumber}", businessGroup, referenceNumber);
            return BadRequest(ApiError.FromMessage("Seqno does not have value"));
        }

        var calculationHistory = await _calculationHistoryRepository.FindByEventTypeAndSeqNumber(businessGroup, referenceNumber, typeResponse.Type, datesAgesResponse.Value().LockedInTransferQuoteSeqno.Value);

        if (calculationHistory.IsNone)
        {
            transferCalculation.Value().UnlockTransferQoute();
            await _mdpDbUnitOfWork.Commit();
            _logger.LogWarning("Calc history record cannnot be found");
            return BadRequest(ApiError.FromMessage("Calc history record cannnot be found"));
        }

        calculationHistory.Value().UpdateIds(edmsResult.Right().ImageId, edmsResult.Right().BatchNumber);

        _postIndexEventsRepository.Add(
                new RetirementPostIndexEvent(
                    businessGroup,
                    referenceNumber,
                    "TRANSFER",
                    edmsResult.Right().BatchNumber,
                    edmsResult.Right().ImageId));

        await _mdpDbUnitOfWork.Commit();
        await _uow.Commit();

        var fileOrError = await _edmsClient.GetDocumentOrError(edmsResult.Right().ImageId);
        if (fileOrError.IsLeft)
            return BadRequest(ApiError.FromMessage(fileOrError.Left().Message));

        return File(fileOrError.Right(), "application/octet-stream", "transfer-journey.pdf");
    }


    [HttpPost("submit-step")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SubmitStep([FromBody] SubmitTransferStepRequest request)
    {
        var now = DateTimeOffset.UtcNow;
        return await _transferJourneyRepository.Find(HttpContext.User.User().BusinessGroup, HttpContext.User.User().ReferenceNumber)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    var result = journey.TrySubmitStep(
                            request.CurrentPageKey,
                            request.NextPageKey,
                            now);

                    if (result.IsLeft)
                    {
                        _logger.LogWarning("Failed to submit transfer step. Error: {message}", result.Left().Message);
                        return BadRequest(ApiError.FromMessage(result.Left().Message));
                    }

                    await _mdpDbUnitOfWork.Commit();
                    return NoContent();
                },
                () => BadRequest(ApiError.FromMessage("Member did not start transfer journey yet")));
    }

    [HttpPost("submit-question-step")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SubmitQuestionStep([FromBody] SubmitTransferQuestionStepRequest request)
    {
        var now = DateTimeOffset.UtcNow;
        return await _transferJourneyRepository.Find(HttpContext.User.User().BusinessGroup, HttpContext.User.User().ReferenceNumber)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    var result = journey.TrySubmitStep(
                            request.CurrentPageKey,
                            request.NextPageKey,
                            now,
                            request.QuestionKey,
                            request.AnswerKey);

                    if (result.IsLeft)
                    {
                        _logger.LogWarning("Failed to submit transfer question step. Error: {message}", result.Left().Message);
                        return BadRequest(ApiError.FromMessage(result.Left().Message));
                    }

                    await _mdpDbUnitOfWork.Commit();
                    return NoContent();
                },
                () => BadRequest(ApiError.FromMessage("Member did not start transfer journey yet")));
    }

    [HttpGet("previous-step/{currentPageKey}")]
    [ProducesResponseType(typeof(TransferPreviousStepResponse), 200)]
    [SwaggerResponse(404, "Not Found. Available error codes: previous_page_key_not_found", typeof(ApiError))]
    public async Task<IActionResult> PreviousStep([MinLength(2), MaxLength(25)] string currentPageKey)
    {
        return (await _transferJourneyRepository.Find(HttpContext.User.User().BusinessGroup, HttpContext.User.User().ReferenceNumber))
            .Match<IActionResult>(journey =>
            {
                var result = journey.PreviousStep(currentPageKey);
                if (result.IsNone)
                    return NotFound(ApiError.From(
                        "Previous step does not exist in transfer journey for given current page key.",
                        "previous_page_key_not_found"));

                return Ok(TransferPreviousStepResponse.From(result.Value()));
            },
            () => NotFound(ApiError.From(
                            "Transfer journey does not exist for given member",
                            "previous_page_key_not_found")));
    }

    [HttpGet("question-form/{currentPageKey}")]
    [ProducesResponseType(typeof(TransferQuestionFormResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> QuestionForm([MinLength(2), MaxLength(25)] string currentPageKey)
    {
        return (await _transferJourneyRepository.Find(HttpContext.User.User().BusinessGroup, HttpContext.User.User().ReferenceNumber))
            .Match<IActionResult>(
                journey =>
                {
                    var result = journey.QuestionForm(currentPageKey);

                    if (result.IsNone)
                        return NotFound(ApiError.NotFound());

                    return Ok(TransferQuestionFormResponse.From(result.Value()));
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("integrity/{pageKey}")]
    [ProducesResponseType(typeof(TransferIntegrityResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> CheckJourneyIntegrity([MinLength(2), MaxLength(25)] string pageKey)
    {
        return await _transferJourneyRepository.Find(HttpContext.User.User().BusinessGroup, HttpContext.User.User().ReferenceNumber)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    journey.RemoveDeadEndSteps();
                    await _mdpDbUnitOfWork.Commit();
                    return Ok(TransferIntegrityResponse.From(journey.GetRedirectStepPageKey(pageKey)));
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpPost("contact")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> SubmitContact([FromBody] TransferJourneyContactRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        return await _transferJourneyRepository.Find(businessGroup, referenceNumber)
             .ToAsync()
             .MatchAsync<IActionResult>(async journey =>
             {
                 var emailOrError = request.Email != null ? Email.Create(request.Email) : Email.Empty();
                 if (emailOrError.IsLeft)
                     return BadRequest(ApiError.FromMessage(emailOrError.Left().Message));

                 var phoneOrError = request.PhoneNumber == null || request.PhoneCode == null ? Phone.Empty() : Phone.Create(request.PhoneCode, request.PhoneNumber);
                 if (phoneOrError.IsLeft)
                 {
                     _logger.LogInformation("Invalid phone provided. Error: {message}", phoneOrError.Left().Message);
                     return BadRequest(ApiError.FromMessage(phoneOrError.Left().Message));
                 }

                 var contactOrError = _transferJourneyContactFactory.Create(
                     request.Name,
                     request.AdvisorName,
                     request.CompanyName,
                     emailOrError.Right(),
                     phoneOrError.Right(),
                     request.Type,
                     request.SchemeName,
                     DateTimeOffset.UtcNow);
                 if (contactOrError.IsLeft)
                 {
                     _logger.LogInformation("Invalid contact details provided. Error: {message}", contactOrError.Left().Message);
                     return BadRequest(ApiError.FromMessage(contactOrError.Left().Message));
                 }

                 journey.SubmitContact(contactOrError.Right());

                 await _mdpDbUnitOfWork.Commit();

                 return NoContent();
             },
             () => NotFound(ApiError.NotFound()));
    }

    [HttpPost("contact/address")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> SubmitContactAddress([FromBody] TransferJourneyContactAddressRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        return await _transferJourneyRepository.Find(businessGroup, referenceNumber)
             .ToAsync()
             .MatchAsync<IActionResult>(async journey =>
             {
                 var addressOrError = Address.Create(request.Line1,
                        request.Line2,
                        request.Line3,
                        request.Line4,
                        request.Line5,
                        request.Country,
                        request.CountryCode,
                        request.PostCode);

                 if (addressOrError.IsLeft)
                 {
                     _logger.LogInformation("Invalid address details provided. Error: {message}", addressOrError.Left().Message);
                     return BadRequest(ApiError.FromMessage(addressOrError.Left().Message));
                 }

                 var error = journey.SubmitContactAddress(
                     addressOrError.Right(),
                     request.Type);
                 if (error.HasValue)
                 {
                     _logger.LogInformation("Failed to submit address. Error: {message}", error.Value.Message);
                     return BadRequest(ApiError.FromMessage(error.Value.Message));
                 }

                 await _mdpDbUnitOfWork.Commit();

                 return NoContent();
             },
             () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("transfer-application-status")]
    [ProducesResponseType(typeof(TransferApplicationStatusResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> GetTransferApplicationStatus()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        return await _memberRepository.FindMember(referenceNumber, businessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async member =>
                {
                    var transferApplicationStatus = member.GetTransferApplicationStatus(await _transferCalculationRepository.Find(businessGroup, referenceNumber));
                    return Ok(TransferApplicationStatusResponse.From(transferApplicationStatus));
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpPost("transfer-application-status")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> SubmitTaStatus([FromBody] TransferApplicationStatusRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        var transferJourney = await _transferJourneyRepository.Find(businessGroup, referenceNumber);
        if (transferJourney.IsNone)
            return NotFound(ApiError.NotFound());

        transferJourney.Value().RemoveStepsAndUpdateLastStepCurrentPageKeyToHub();

        return await _transferCalculationRepository.Find(businessGroup, referenceNumber)
             .ToAsync()
             .MatchAsync<IActionResult>(async calculation =>
             {
                 var error = calculation.SetStatus(request.Status);
                 if (error.HasValue)
                 {
                     _logger.LogWarning("Failed to set transfer application status. Error: {message}", error.Value.Message);
                     return BadRequest(ApiError.FromMessage(error.Value.Message));
                 }

                 await _mdpDbUnitOfWork.Commit();
                 return NoContent();
             },
             () => NotFound(ApiError.NotFound()));
    }

    [HttpPost("gbg/{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> SaveGbgId(Guid id)
    {
        return await _transferJourneyRepository
            .Find(HttpContext.User.User().BusinessGroup, HttpContext.User.User().ReferenceNumber)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    journey.SaveGbgId(id);
                    await _mdpDbUnitOfWork.Commit();
                    return NoContent();
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("transfer-application")]
    [ProducesResponseType(typeof(TransferApplicationResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> TransferApplication()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        return await _transferJourneyRepository.Find(businessGroup, referenceNumber)
             .ToAsync()
             .MatchAsync<IActionResult>(
                async journey =>
                {
                    var member = (await _memberRepository.FindMember(referenceNumber, businessGroup)).Value();
                    var transferCalculation = (await _transferCalculationRepository.Find(businessGroup, referenceNumber)).Value();
                    if (transferCalculation.TransferQuoteJson == null)
                        return Ok(new TransferApplicationResponse(journey, member.GetTransferApplicationStatus(transferCalculation)));

                    var transferQuote = _calculationsParser.GetTransferQuote(transferCalculation.TransferQuoteJson);

                    return Ok(new TransferApplicationResponse(journey, transferQuote, member.GetTransferApplicationStatus(transferCalculation)));
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpPut("delete-application")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> DeleteApplication()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        return await _transferJourneyRepository.Find(businessGroup, referenceNumber)
             .ToAsync()
             .MatchAsync<IActionResult>(
                async journey =>
                {
                    var transferCalculation = await _transferCalculationRepository.Find(businessGroup, referenceNumber);
                    var error = transferCalculation.Value().SetStatus(TransferApplicationStatus.StartedTA);
                    if (error.HasValue)
                    {
                        _logger.LogWarning("Failed to set transfer application status. Error: {message}", error.Value.Message);
                        return BadRequest(ApiError.FromMessage(error.Value.Message));
                    }

                    var documents = await _journeyDocumentsRepository.List(businessGroup, referenceNumber, new List<string> { MdpConstants.JourneyTypeTransferV2, MdpConstants.IdentityDocumentType });
                    journey.RemoveInactiveBranches();
                    journey.ReplaceAllStepsTo(JourneyStep.Create("hub", "t2_guaranteed_value_2", DateTimeOffset.UtcNow));
                    journey.RemoveAllContacts();
                    journey.ClearPensionWiseDate();
                    journey.ClearFinancialAdviseDate();
                    journey.ClearFlexibleBenefitsData();
                    _journeyDocumentsRepository.RemoveAll(documents);
                    await _mdpDbUnitOfWork.Commit();
                    return NoContent();
                },
                () => NotFound(ApiError.NotFound()));
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

                    var document = new UploadedDocument(referenceNumber, businessGroup, "transfer2", null, SubmitPdfFileName, edmsResult.Right().Uuid, DocumentSource.Outgoing, true, "TRNAPP");
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
                () => base.NotFound(ApiError.NotFound()));
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

    [HttpGet("download/pdf")]
    [ProducesResponseType(typeof(FileResult), 200, "application/octet-stream")]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GetPdf([Required][FromQuery] string contentAccessKey)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        return await _transferJourneyRepository.Find(businessGroup, referenceNumber)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    if (journey.TransferSummaryImageId == null)
                        return BadRequest(ApiError.FromMessage("Transfer journey summary imageId does not exists"));

                    var fileOrError = await _edmsClient.GetDocumentOrError(journey.TransferSummaryImageId.Value);
                    if (fileOrError.IsLeft)
                    {
                        _logger.LogWarning("Failed to download document with {documentId}. Error: {Message}", journey.TransferImageId, fileOrError.Left().Message);
                        return BadRequest(ApiError.FromMessage(fileOrError.Left().Message));
                    }

                    return File(fileOrError.Right().ToByteArray(), "application/pdf", "TransferApplicationSummary.pdf");
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("download/newPdf")]
    [ProducesResponseType(typeof(FileResult), 200, "application/octet-stream")]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GetNewPdf([Required][FromQuery] string contentAccessKey)
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
                    var documents = await _journeyDocumentsRepository.List(businessGroup, referenceNumber, "transfer2");
                    var retirementCalculation = await _calculationsRepository.Find(referenceNumber, businessGroup);
                    var pdfStream = await RenderSubmitPdfSummary(journey, member.Value(), transferQuote, transferCalculation, contentAccessKey, documents, retirementCalculation);

                    if (pdfStream == null)
                    {
                        _logger.LogWarning("Failed to generate pdf for user {referenceNumber}, business group {businessGroup}", referenceNumber, businessGroup);
                        return NotFound(ApiError.NotFound());
                    }

                    return File(pdfStream.ToArray(), "application/pdf", "TransferApplicationSummary.pdf");
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpPost("remove-steps-from/{pageKey}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> RemoveSteps(string pageKey)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        return await _transferJourneyRepository.Find(businessGroup, referenceNumber)
             .ToAsync()
             .MatchAsync<IActionResult>(
                async journey =>
                {
                    journey.MarkNextPageAsDeadEnd(journey.PreviousStep(pageKey).Value());
                    await _mdpDbUnitOfWork.Commit();
                    return NoContent();
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("pension-tranches")]
    [ProducesResponseType(typeof(TransferValueResponseV2), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> PensionTranchesV2([FromQuery] PensionTranchesRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        return (await _calculationsClient.PensionTranches(businessGroup, referenceNumber, request.RequestedTransferValue))
            .Match<IActionResult>(
                response => Ok(new TransferValueResponseV2(response)),
                error =>
                {
                    _logger.LogError("Failed to calculate partial pension tranches. Error: {message}", error.Message);
                    return BadRequest(ApiError.FromMessage(error.Message));
                });
    }

    [HttpGet("transfer-values")]
    [ProducesResponseType(typeof(PensionIncomeResponseV2), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> TransferValuesV2([FromQuery] TransferValuesRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        return (await _calculationsClient.TransferValues(businessGroup, referenceNumber, request.RequestedResidualPension))
            .Match<IActionResult>(
                response => Ok(new PensionIncomeResponseV2(response)),
                error =>
                {
                    _logger.LogError("Failed to calculate partial transfer values. Error: {message}", error.Message);
                    return BadRequest(ApiError.FromMessage(error.Message));
                });
    }

    [HttpPost("documents/post-submission")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> AddPostSubmissionDocuments()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        return await _transferJourneyRepository.Find(businessGroup, referenceNumber)
             .ToAsync()
             .MatchAsync<IActionResult>(
                async journey =>
                {
                    var result = await _journeyDocumentsHandlerService.PostIndex(businessGroup, referenceNumber, journey.CaseNumber, MdpConstants.JourneyTypeTransferV2);
                    if (result.IsLeft)
                        return BadRequest(ApiError.FromMessage(result.Left().Message));

                    await _mdpDbUnitOfWork.Commit();
                    return NoContent();
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpPost("flexible-benefits")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SaveFlexibleBenefits([FromBody] FlexibleBenefitsRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        return await _transferJourneyRepository.Find(businessGroup, referenceNumber)
            .ToAsync()
            .MatchAsync<IActionResult>(
              async journey =>
              {
                  var error = journey.SaveFlexibleBenefits(request.NameOfPlan, request.TypeOfPayment, request.DateOfPayment, DateTimeOffset.UtcNow);
                  if (error.HasValue)
                  {
                      _logger.LogError("Flexible benefits validation error. Error: {message}", error.Value.Message);
                      return BadRequest(ApiError.FromMessage(error.Value.Message));
                  }

                  await _mdpDbUnitOfWork.Commit();
                  return NoContent();
              },
              () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("flexible-benefits")]
    [ProducesResponseType(typeof(FlexibleBenefitsResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> FlexibleBenefits()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        return await _transferJourneyRepository.Find(businessGroup, referenceNumber)
            .ToAsync()
            .MatchAsync<IActionResult>(
              async journey => Ok(new FlexibleBenefitsResponse(journey.NameOfPlan, journey.TypeOfPayment, journey.DateOfPayment)),
              () => NotFound(ApiError.NotFound()));
    }

    [HttpPost("submit-pension-wise")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SubmitPensionWiseDate([FromBody] SubmitTransferPensionWiseRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        return await _transferJourneyRepository.Find(businessGroup, referenceNumber)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    var transferCalculation = (await _transferCalculationRepository.Find(businessGroup, referenceNumber)).Value();
                    if (transferCalculation.Status != TransferApplicationStatus.SubmitStarted)
                    {
                        _logger.LogError("Transfer journey status must be SubmitStarted for user {referenceNumber}, business group {businessGroup}. Current transfer status: {status}", referenceNumber, businessGroup, transferCalculation.Status);
                        return BadRequest(ApiError.FromMessage("Transfer journey must be started"));
                    }

                    var result = journey.SetPensionWiseDate(request.PensionWiseDate.Value);
                    if (result.HasValue)
                    {
                        _logger.LogError("Pension wise date validation error. Error: {message}", result.Value.Message);
                        return BadRequest(ApiError.FromMessage(result.Value.Message));
                    }

                    await _mdpDbUnitOfWork.Commit();
                    return NoContent();
                },
                () => NotFound(ApiError.FromMessage("Transfer journey does not exists")));
    }

    [HttpGet("pension-wise")]
    [ProducesResponseType(typeof(TransferPensionWiseResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> PensionWise()
    {
        return (await _transferJourneyRepository.Find(HttpContext.User.User().BusinessGroup,
                HttpContext.User.User().ReferenceNumber))
            .Match<IActionResult>(
                journey => Ok(new TransferPensionWiseResponse(journey)),
                () => NotFound(ApiError.NotFound()));
    }

    [HttpPost("submit-financial-advise")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SubmitFinancialAdviseDate([FromBody] SubmitTransferFinancialAdviseRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        return await _transferJourneyRepository.Find(businessGroup, referenceNumber)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    var transferCalculation = (await _transferCalculationRepository.Find(businessGroup, referenceNumber)).Value();
                    if (transferCalculation.Status != TransferApplicationStatus.SubmitStarted)
                    {
                        _logger.LogError("Transfer journey status must be SubmitStarted for user {referenceNumber}, business group {businessGroup}. Current transfer status: {status}", referenceNumber, businessGroup, transferCalculation.Status);
                        return BadRequest(ApiError.FromMessage("Transfer journey must be started"));
                    }

                    var result = journey.SetFinancialAdviseDate(request.FinancialAdviseDate.Value);
                    if (result.HasValue)
                    {
                        _logger.LogError("Financial advise date validation error. Error: {message}", result.Value.Message);
                        return BadRequest(ApiError.FromMessage(result.Value.Message));
                    }

                    await _mdpDbUnitOfWork.Commit();
                    return NoContent();
                },
                () => NotFound(ApiError.FromMessage("Transfer journey does not exists")));
    }

    [HttpGet("financial-advise")]
    [ProducesResponseType(typeof(TransferFinancialAdviseResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> FinancialAdvise()
    {
        return (await _transferJourneyRepository.Find(HttpContext.User.User().BusinessGroup,
                HttpContext.User.User().ReferenceNumber))
            .Match<IActionResult>(
                journey => Ok(new TransferFinancialAdviseResponse(journey)),
                () => NotFound(ApiError.NotFound()));
    }
}