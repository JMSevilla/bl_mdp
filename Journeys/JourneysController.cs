using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Infrastructure.Edms;
using WTW.MdpService.Infrastructure.Journeys;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.MdpService.Infrastructure.MemberDb.Documents;
using WTW.MdpService.Journeys.Submit.Services;
using WTW.MdpService.RetirementJourneys;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Journeys;

[ApiController]
[Route("api/journeys")]
public class JourneysController : ControllerBase
{
    private readonly IJourneysRepository _journeysRepository;
    private readonly IJourneyDocumentsRepository _journeyDocumentsRepository;
    private readonly IMdpUnitOfWork _mdpUnitOfWork;
    private readonly ILogger<JourneysController> _logger;
    private readonly IMemberRepository _memberRepository;
    private readonly IDocumentRenderer _documentRenderer;
    private readonly IDocumentsRendererDataFactory _documentsRendererDataFactory;
    private readonly IGenericJourneyService _genericJourneyService;
    private readonly IJourneyService _journeyService;
    private readonly IEdmsClient _edmsClient;
    private readonly IGenericJourneyDetails _genericJourneyDetails;
    private readonly IDocumentsRepository _documentsRepository;

    public JourneysController(IJourneysRepository journeysRepository,
        IJourneyDocumentsRepository journeyDocumentsRepository,
        IMdpUnitOfWork mdpUnitOfWork,
        ILogger<JourneysController> logger,
        IMemberRepository memberRepository,
        IDocumentRenderer documentRenderer,
        IDocumentsRendererDataFactory documentsRendererDataFactory,
        IGenericJourneyService genericJourneyService,
        IJourneyService journeyService,
        IEdmsClient edmsClient,
        IGenericJourneyDetails genericJourneyDetails,
        IDocumentsRepository documentsRepository)
    {
        _journeysRepository = journeysRepository;
        _journeyDocumentsRepository = journeyDocumentsRepository;
        _mdpUnitOfWork = mdpUnitOfWork;
        _logger = logger;
        _memberRepository = memberRepository;
        _documentRenderer = documentRenderer;
        _documentsRendererDataFactory = documentsRendererDataFactory;
        _genericJourneyService = genericJourneyService;
        _journeyService = journeyService;
        _edmsClient = edmsClient;
        _genericJourneyDetails = genericJourneyDetails;
        _documentsRepository = documentsRepository;
    }

    [HttpPost("{journeyType}/start")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Start(string journeyType, [FromBody] StartJourneyRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        return await _journeysRepository.Find(businessGroup, referenceNumber, journeyType)
            .ToAsync()
            .MatchAsync<IActionResult>(
            _ => NoContent(),
            async () =>
            {
                var newJourney = await _genericJourneyService.CreateJourney(businessGroup,
                    referenceNumber,
                    journeyType,
                    request.CurrentPageKey,
                    request.NextPageKey,
                    request.RemoveOnLogin,
                    request.JourneyStatus);
                await _journeysRepository.Create(newJourney);
                await _mdpUnitOfWork.Commit();
                return NoContent();
            });
    }

    [HttpPost("{journeyType}/submit-step")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SubmitStep(string journeyType, [FromBody] SubmitGenericStepRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        return await _journeysRepository.Find(businessGroup, referenceNumber, journeyType)
            .ToAsync()
            .MatchAsync<IActionResult>(
            async journey =>
            {
                var result = journey.TrySubmitStep(request.CurrentPageKey, request.NextPageKey, request.JourneyStatus, DateTimeOffset.UtcNow);
                if (result.IsLeft)
                {
                    _logger.LogWarning("Failed to submit step. Error: {error}", result.Left().Message);
                    return BadRequest(ApiError.FromMessage(result.Left().Message));
                }

                journey.RenewStepUpdatedDate(request.CurrentPageKey, request.NextPageKey, DateTimeOffset.UtcNow);

                await _mdpUnitOfWork.Commit();
                return NoContent();
            },
            () =>
            {
                _logger.LogWarning("{journeyType} journey  does not exist for member {businessGroup}:{referenceNumber}.", journeyType, businessGroup, referenceNumber);
                return NotFound(ApiError.FromMessage($"Journey must be started to submit step. Journey type: {journeyType}."));
            });
    }

    [HttpGet("{journeyType}/{currentPageKey}")]
    [ProducesResponseType(typeof(PreviousStepResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> PreviousStep(string journeyType, [MinLength(2), MaxLength(25)] string currentPageKey)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        return await _journeysRepository.Find(businessGroup, referenceNumber, journeyType)
            .ToAsync()
            .Match<IActionResult>(journey =>
            {
                var result = journey.PreviousStep(currentPageKey);
                if (result.IsNone)
                {
                    _logger.LogWarning("Previous step does not exist in journey for given current page key: {currentPageKey}. Journey type: {journeyType}.", currentPageKey, journeyType);
                    return NotFound(ApiError.FromMessage($"Previous step does not exist in journey for given current page key: {currentPageKey}. Journey type: {journeyType}."));
                }
                return Ok(PreviousStepResponse.From(result.Value()));
            },
            () =>
            {
                _logger.LogWarning("{journeyType} journey  does not exist for member {businessGroup}:{referenceNumber}.", journeyType, businessGroup, referenceNumber);
                return NotFound(ApiError.FromMessage($"Journey does not exist for given member. Journey type: {journeyType}."));
            });
    }

    [HttpGet("{journeyType}/integrity/{pageKey}")]
    [ProducesResponseType(typeof(IntegrityResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> CheckJourneyIntegrity(string journeyType, [MinLength(2), MaxLength(25)] string pageKey)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        return (await _journeysRepository.Find(businessGroup, referenceNumber, journeyType))
            .Match<IActionResult>(
                journey => Ok(IntegrityResponse.From(journey.GetRedirectStepPageKey(pageKey))),
                () =>
                {
                    _logger.LogWarning("{journeyType} journey  does not exist for member {businessGroup}:{referenceNumber}.", journeyType, businessGroup, referenceNumber);
                    return NotFound(ApiError.FromMessage($"Journey does not exist for given member. Journey type: {journeyType}."));
                });
    }

    [HttpDelete("{journeyType}/delete")]
    [ProducesResponseType(typeof(IntegrityResponse), 204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> Delete(string journeyType)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        return await _journeysRepository.Find(businessGroup, referenceNumber, journeyType)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    _journeysRepository.Remove(journey);

                    var documents = await _journeyDocumentsRepository.List(businessGroup, referenceNumber, journeyType);
                    if (documents.Any())
                    {
                        _logger.LogWarning("Transfer journey uploaded documents will be deleted. Uuids: {uuids}", string.Join(",", documents.Select(x => x.Uuid)));
                        _journeyDocumentsRepository.RemoveAll(documents);
                    }

                    await _mdpUnitOfWork.Commit();
                    return NoContent();
                },
                () =>
                {
                    _logger.LogWarning("{journeyType} journey  does not exist for member {businessGroup}:{referenceNumber}.", journeyType, businessGroup, referenceNumber);
                    return NotFound(ApiError.FromMessage($"Journey does not exist for given member. Journey type: {journeyType}."));
                });
    }

    [HttpGet("{journeyType}/question-form/{currentPageKey}")]
    [ProducesResponseType(typeof(JourneyQuestionFormResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> QuestionForm(string journeyType, [MinLength(2), MaxLength(25)] string currentPageKey)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        return (await _journeysRepository.Find(businessGroup, referenceNumber, journeyType))
            .Match<IActionResult>(
                journey =>
                {
                    var result = journey.QuestionForm(currentPageKey);
                    if (result.IsNone)
                    {
                        _logger.LogWarning("QuestionForm with current page key {currentPageKey} can not be found for journey type {journeyType}", currentPageKey, journeyType);
                        return NotFound(ApiError.NotFound());
                    }

                    return Ok(new JourneyQuestionFormResponse(result.Value()));
                },
                () =>
                {
                    _logger.LogWarning("{journeyType} journey  does not exist for member {businessGroup}:{referenceNumber}.", journeyType, businessGroup, referenceNumber);
                    return NotFound(ApiError.FromMessage($"Journey does not exist for given member. Journey type: {journeyType}."));
                });
    }

    [HttpPost("{journeyType}/submit-question-step")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> SubmitQuestionStep(string journeyType, [FromBody] SubmitJourneyQuestionStepRequest request)
    {
        var now = DateTimeOffset.UtcNow;
        return await _journeysRepository.Find(HttpContext.User.User().BusinessGroup, HttpContext.User.User().ReferenceNumber, journeyType)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    var result = journey.TrySubmitStep(
                            request.CurrentPageKey,
                            request.NextPageKey,
                            now,
                            request.QuestionKey,
                            request.AnswerKey,
                            request.AnswerValue);

                    if (result.IsLeft)
                    {
                        _logger.LogError("Failed to submit {journeyType} question step. Error: {message}", journeyType, result.Left().Message);
                        return BadRequest(ApiError.FromMessage(result.Left().Message));
                    }
                    journey.RenewStepUpdatedDate(request.CurrentPageKey, request.NextPageKey, now);
                    await _mdpUnitOfWork.Commit();
                    return NoContent();
                },
                () =>
                {
                    _logger.LogWarning("Member did not start {journeyType} journey yet", journeyType);
                    return NotFound(ApiError.FromMessage($"Member did not start {journeyType} journey yet"));
                });
    }

    [HttpGet("{journeyType}/data")]
    [ProducesResponseType(typeof(JourneyDataResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GetAllJourneyData(string journeyType)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var member = await _memberRepository.FindMember(referenceNumber, businessGroup);
        if (member.IsNone)
        {
            _logger.LogError("Member {businessGroup}:{referenceNumber} not found", businessGroup, referenceNumber);
            return BadRequest(ApiError.FromMessage("Member was not found."));
        }

        var journeyData = await _genericJourneyDetails.GetAll(businessGroup, referenceNumber, journeyType);
        if (journeyData.IsNone)
        {
            _logger.LogWarning("{journeyType} journey  does not exist for member {businessGroup}:{referenceNumber}.", journeyType, businessGroup, referenceNumber);
            return NotFound(ApiError.FromMessage($"Journey does not exist for given member. Journey type: {journeyType}."));
        }
        var journeyDocuments = await _journeyDocumentsRepository.List(businessGroup, referenceNumber, journeyType);

        return Ok(new JourneyDataResponse(member.Value(), journeyData.Value(), journeyDocuments));
    }

    [HttpPost("{journeyType}/stage/status")]
    [ProducesResponseType(typeof(JourneyStageStatusResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> StagesStatus(string journeyType, [FromBody] JourneyStageStatusRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        return await _journeysRepository.Find(businessGroup, referenceNumber, journeyType)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    var member = await _memberRepository.FindMember(referenceNumber, businessGroup);
                    if (member.IsNone)
                    {
                        _logger.LogError("Member {businessGroup}:{referenceNumber} not found", businessGroup, referenceNumber);
                        return BadRequest(ApiError.FromMessage("Member was not found."));
                    }

                    var result = journey.GetStageStatus(request.Stages.Map(x => new GenericJourneyStage { Stage = x.Stage, Page = new GenericJourneyStagePage { StageEndSteps = x.Page.End, StageStartSteps = x.Page.Start } }));
                    return Ok(result.Select(x => new JourneyStageStatusResponse(x)));
                },
                () =>
                {
                    _logger.LogWarning("{journeyType} journey  does not exist for member {businessGroup}:{referenceNumber}.", journeyType, businessGroup, referenceNumber);
                    return NotFound(ApiError.FromMessage($"Journey does not exist for given member. Journey type: {journeyType}."));
                });
    }
   
    [HttpGet("{journeyType}/pdf/download")]
    [ProducesResponseType(typeof(FileResult), 200, "application/octet-stream")]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GenerateJourneyPdf(string journeyType, [FromQuery] JourneyPdfDownloadRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        return await _journeyService.GetJourney(journeyType, businessGroup, referenceNumber)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    if (!await _memberRepository.ExistsMember(referenceNumber, businessGroup))
                    {
                        _logger.LogError("Member {businessGroup}:{referenceNumber} not found", businessGroup, referenceNumber);
                        return BadRequest(ApiError.FromMessage("Member was not found."));
                    }

                    var submissionDetailsOrError = await _genericJourneyService.GetSubmissionDetailsFromGenericData(businessGroup, referenceNumber, journeyType);
                    if (submissionDetailsOrError.IsRight && submissionDetailsOrError.Right().SummaryPdfEdmsImageId.HasValue)
                    {
                        var memberDocument = await _documentsRepository.FindByImageId(referenceNumber, businessGroup, submissionDetailsOrError.Right().SummaryPdfEdmsImageId.Value);
                        var file = await _edmsClient.GetDocument(submissionDetailsOrError.Right().SummaryPdfEdmsImageId.Value);
                        return File(file, "application/pdf", memberDocument.Value().FileName);
                    }

                    var pdfSummaryRendererData = _documentsRendererDataFactory.CreateForDirectPdfDownload(journeyType, request.TemplateName, businessGroup, referenceNumber, request.ContentAccessKey);
                    var document = await _documentRenderer.RenderDirectPdf(
                        pdfSummaryRendererData,
                        Request.Headers[HeaderNames.Authorization],
                        Request.Headers["env"]);

                    if (document.PdfStream == null)
                    {
                        _logger.LogWarning("Failed to generate pdf for user {referenceNumber}, business group {businessGroup}", referenceNumber, businessGroup);
                        return NotFound(ApiError.NotFound());
                    }

                    return File(document.PdfStream.ToArray(), "application/pdf", document.FileName);
                },
                () =>
                {
                    _logger.LogWarning("{journeyType} journey  does not exist for member {businessGroup}:{referenceNumber}.", journeyType, businessGroup, referenceNumber);
                    return NotFound(ApiError.FromMessage($"Journey does not exist for given member. Journey type: {journeyType}."));
                });
    }

    [HttpGet("pdf/generate")]
    [ProducesResponseType(typeof(FileResult), 200, "application/octet-stream")]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GeneratePdf([FromQuery] GeneratePdfRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        DocumentsRendererData documentsRendererData = null;

        if (!string.IsNullOrEmpty(request.JourneyType))
        {
            documentsRendererData = _documentsRendererDataFactory.CreateForSubmit(
                request.JourneyType,
                businessGroup,
                referenceNumber,
                request.ContentAccessKey,
                request.CaseNumber);
        }
        else if (!string.IsNullOrEmpty(request.CaseType))
        {
            documentsRendererData = _documentsRendererDataFactory.CreateForQuoteRequest(
                businessGroup,
                referenceNumber, request.CaseNumber, request.ContentAccessKey, request.CaseType);
        }

        if (!string.IsNullOrEmpty(request.JourneyType) || !string.IsNullOrEmpty(request.CaseType))
        {
            var document = await _documentRenderer.RenderGenericSummaryPdf(
            documentsRendererData,
            Request.Headers[HeaderNames.Authorization],
            Request.Headers["env"]);

            if (document.PdfStream == null)
            {
                _logger.LogWarning("Failed to generate pdf for user {referenceNumber}, business group {businessGroup}", referenceNumber, businessGroup);
                return NotFound(ApiError.NotFound());
            }

            return File(document.PdfStream.ToArray(), "application/pdf", document.FileName);
        }

        return BadRequest(ApiError.FromMessage("Invalid request, Journey / Case Type not supplied."));
    }
}