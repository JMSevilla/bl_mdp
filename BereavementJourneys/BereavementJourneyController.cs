using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using WTW.MdpService.Domain.Bereavement;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Infrastructure.BereavementDb;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.Edms;
using WTW.MdpService.Infrastructure.EdmsIndexing;
using WTW.MdpService.Infrastructure.EmailConfirmation;
using WTW.MdpService.Infrastructure.Geolocation;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.MdpService.Infrastructure.PdfGenerator;
using WTW.MdpService.Infrastructure.Templates.Bereavement;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;
using WTW.Web.Templates;

namespace WTW.MdpService.BereavementJourneys;

[ApiController]
[Route("api/bereavement-journeys")]
[Authorize(Policy = "BereavementInitialUserOrMember")]
public class BereavementJourneyController : ControllerBase
{
    private readonly IBereavementJourneyRepository _repository;
    private readonly BereavementJourneyConfiguration _journeyConfiguration;
    private readonly IEmailConfirmationSmtpClient _smtpClient;
    private readonly ILoqateApiClient _loqateApiClient;
    private readonly IContentClient _contentClient;
    private readonly IEdmsDocumentsIndexing _edmsDocumentsIndexing;
    private readonly IBereavementCase _bereavementCase;
    private readonly IBereavementUnitOfWork _bereavementUnitOfWork;
    private readonly IMdpUnitOfWork _mdpUnitOfWork;
    private readonly IJourneyDocumentsRepository _journeyDocumentsRepository;
    private readonly IBereavementTemplate _bereavementTemplate;
    private readonly IEdmsClient _edmsClient;
    private readonly ILogger<BereavementJourneyController> _logger;
    private readonly IMemberDbUnitOfWork _uow;
    private readonly IPdfGenerator _pdfGenerator;

    public BereavementJourneyController(IBereavementJourneyRepository repository,
        BereavementJourneyConfiguration journeyConfiguration,
        IEmailConfirmationSmtpClient smtpClient,
        ILoqateApiClient loqateApiClient,
        IContentClient contentClient,
        IEdmsDocumentsIndexing edmsDocumentsIndexing,
        IBereavementCase bereavementCase,
        IBereavementUnitOfWork bereavementUnitOfWork,
        IMdpUnitOfWork mdpUnitOfWork,
        IJourneyDocumentsRepository journeyDocumentsRepository,
        IBereavementTemplate bereavementTemplate, IEdmsClient edmsClient,
        ILogger<BereavementJourneyController> logger,
        IMemberDbUnitOfWork uow,
        IPdfGenerator pdfGenerator)
    {
        _repository = repository;
        _journeyConfiguration = journeyConfiguration;
        _smtpClient = smtpClient;
        _loqateApiClient = loqateApiClient;
        _contentClient = contentClient;
        _edmsDocumentsIndexing = edmsDocumentsIndexing;
        _bereavementCase = bereavementCase;
        _bereavementUnitOfWork = bereavementUnitOfWork;
        _mdpUnitOfWork = mdpUnitOfWork;
        _journeyDocumentsRepository = journeyDocumentsRepository;
        _bereavementTemplate = bereavementTemplate;
        _edmsClient = edmsClient;
        _logger = logger;
        _uow = uow;
        _pdfGenerator = pdfGenerator;
    }

    [HttpPost("start")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> StartBereavementJourney([FromBody] StartBereavementJourneyRequest request)
    {
        var (bereavementReferenceNumber, businessGroup) = HttpContext.User.BereavementUser();
        var utcNow = DateTimeOffset.UtcNow;
        return await _repository.Find(businessGroup, bereavementReferenceNumber)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    if (journey.IsExpired(utcNow))
                    {
                        _repository.Remove(journey);

                        var journeyOrError = BereavementJourney.Create(bereavementReferenceNumber,
                        businessGroup,
                        DateTimeOffset.UtcNow,
                        request.CurrentPageKey,
                        request.NextPageKey,
                        _journeyConfiguration.ValidityPeriodInMin);

                        if (journeyOrError.IsLeft)
                            return BadRequest(ApiError.FromMessage(journeyOrError.Left().Message));

                        await _repository.Create(journeyOrError.Right());
                        await _bereavementUnitOfWork.Commit();
                        return NoContent();
                    }

                    return BadRequest(ApiError.FromMessage("Bereavement journey already created."));
                },
                async () =>
                {
                    var journeyOrError = BereavementJourney.Create(bereavementReferenceNumber,
                        businessGroup,
                        DateTimeOffset.UtcNow,
                        request.CurrentPageKey,
                        request.NextPageKey,
                        _journeyConfiguration.ValidityPeriodInMin);

                    if (journeyOrError.IsLeft)
                        return BadRequest(ApiError.FromMessage(journeyOrError.Left().Message));

                    await _repository.Create(journeyOrError.Right());
                    await _bereavementUnitOfWork.Commit();
                    return NoContent();
                });
    }

    [HttpPost("submit-step")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SubmitStep([FromBody] SubmitBereavementStepRequest request)
    {
        var (bereavementReferenceNumber, businessGroup) = HttpContext.User.BereavementUser();
        var utcNow = DateTimeOffset.UtcNow;
        return await _repository.FindUnexpired(businessGroup, bereavementReferenceNumber, utcNow)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    var result = journey.TrySubmitStep(
                            request.CurrentPageKey,
                            request.NextPageKey,
                            utcNow);

                    if (result.IsLeft)
                        return BadRequest(ApiError.FromMessage(result.Left().Message));

                    await _bereavementUnitOfWork.Commit();
                    return NoContent();
                },
                () => BadRequest(ApiError.FromMessage("Member did not start bereavement journey yet")));
    }

    [HttpPost("submit-question-step")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SubmitQuestionStep([FromBody] SubmitBereavementQuestionStepRequest request)
    {
        var (bereavementReferenceNumber, businessGroup) = HttpContext.User.BereavementUser();
        var utcNow = DateTimeOffset.UtcNow;
        return await _repository.FindUnexpired(businessGroup, bereavementReferenceNumber, utcNow)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    var result = journey.TrySubmitStep(
                            request.CurrentPageKey,
                            request.NextPageKey,
                            utcNow,
                            request.QuestionKey,
                            request.AnswerKey,
                            request.AnswerValue,
                            request.AvoidBranching);

                    if (result.IsLeft)
                        return BadRequest(ApiError.FromMessage(result.Left().Message));

                    await _bereavementUnitOfWork.Commit();
                    return NoContent();
                },
                () => BadRequest(ApiError.FromMessage("Member did not start bereavement journey yet")));
    }

    [HttpGet("question-form/{currentPageKey}")]
    [ProducesResponseType(typeof(BereavementQuestionFormResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> QuestionForm([MinLength(2), MaxLength(25)] string currentPageKey)
    {
        var (bereavementReferenceNumber, businessGroup) = HttpContext.User.BereavementUser();
        return (await _repository.FindUnexpired(businessGroup, bereavementReferenceNumber, DateTimeOffset.UtcNow))
            .Match<IActionResult>(
                journey =>
                {
                    var result = journey.QuestionForm(currentPageKey);

                    if (result.IsNone)
                        return NotFound(ApiError.NotFound());

                    return Ok(BereavementQuestionFormResponse.From(result.Value()));
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("integrity/{pageKey}")]
    [ProducesResponseType(typeof(BereavementIntegrityResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> CheckJourneyIntegrity([MinLength(2), MaxLength(25)] string pageKey)
    {
        var (bereavementReferenceNumber, businessGroup) = HttpContext.User.BereavementUser();
        return (await _repository.FindUnexpired(businessGroup, bereavementReferenceNumber, DateTimeOffset.UtcNow))
            .Match<IActionResult>(
                journey => Ok(BereavementIntegrityResponse.From(journey.GetRedirectStepPageKey(pageKey))),
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("previous-step/{currentPageKey}")]
    [ProducesResponseType(typeof(BereavementPreviousStepResponse), 200)]
    [SwaggerResponse(404, "Not Found. Available error codes: previous_page_key_not_found", typeof(ApiError))]
    public async Task<IActionResult> PreviousStep([MinLength(2), MaxLength(25)] string currentPageKey)
    {
        var (bereavementReferenceNumber, businessGroup) = HttpContext.User.BereavementUser();
        return (await _repository.FindUnexpired(businessGroup, bereavementReferenceNumber, DateTimeOffset.UtcNow))
            .Match<IActionResult>(journey =>
            {
                var result = journey.PreviousStep(currentPageKey);
                if (result.IsNone)
                    return NotFound(ApiError.From(
                        "Previous step does not exist in bereavement journey for given current page key.",
                        "previous_page_key_not_found"));

                return Ok(BereavementPreviousStepResponse.From(result.Value()));
            },
            () => NotFound(ApiError.From(
                            "Bereavement journey does not exist for given user",
                            "previous_page_key_not_found")));
    }

    [HttpPost("keep-alive")]
    [ProducesResponseType(typeof(BereavementJourneyKeepAliveResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> KeepAlive()
    {
        var (bereavementReferenceNumber, businessGroup) = HttpContext.User.BereavementUser();
        return await _repository.FindUnexpired(businessGroup, bereavementReferenceNumber, DateTimeOffset.UtcNow)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    journey.UpdateExpiryDate(DateTimeOffset.UtcNow.AddMinutes(_journeyConfiguration.ValidityPeriodInMin));
                    await _bereavementUnitOfWork.Commit();

                    return Ok(BereavementJourneyKeepAliveResponse.From(journey.ExpirationDate));
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpPost("submit")]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(BereavementJourneySubmitResponse), 200)]
    [Authorize(Policy = "BereavementEmailVerifiedUserOrMember")]
    public async Task<IActionResult> Submit([FromBody] BereavementJourneySubmitRequest request)
    {
        var (bereavementReferenceNumber, businessGroup) = HttpContext.User.BereavementUser();
        return await _repository.FindUnexpired(businessGroup, bereavementReferenceNumber, DateTimeOffset.UtcNow)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    journey.UpdateExpiryDate(DateTimeOffset.UtcNow.AddMinutes(_journeyConfiguration.FailedJourneyValidityPeriodInMin));
                    await _bereavementUnitOfWork.Commit();

                    var pdfName = "bereavement_pdf";
                    var template = await _contentClient.FindUnauthorizedTemplate("bereavement_notification", request.TenantUrl);
                    var pdfTemplate = await _contentClient.FindUnauthorizedTemplate(pdfName, request.TenantUrl);

                    var errorOrCaseNumber = await _bereavementCase.Create(businessGroup, request.Deceased.Name, request.Deceased.Surname, request.Deceased.DateOfBirth, request.Deceased.DateOfDeath, request.Deceased.Identification.PensionReferenceNumbers);

                    var documents = await _journeyDocumentsRepository.List(businessGroup, bereavementReferenceNumber.ToString(), "bereavement");
                    var htmlBody = new Template(template.HtmlBody).Apply(new Dictionary<string, string>()
                    {
                        ["token:bereavement_reference_number"] = errorOrCaseNumber.Right(),
                        ["token:reporter_name"] = request.Reporter.Name
                    });

                    var (_, renderHtml) = await _bereavementTemplate.RenderHtml(
                        pdfTemplate.HtmlBody,
                        journey.JourneyQuestions(),
                        request.Deceased,
                        request.Reporter,
                        request.NextOfKin,
                        request.Executor,
                        request.ContactPerson,
                        DateTimeOffset.UtcNow,
                        documents);
                    await using var stream = await _pdfGenerator.Generate(renderHtml, pdfTemplate.HtmlHeader, pdfTemplate.HtmlFooter);

                    if (stream == null || stream.Length == 0)
                    {
                        string error = stream == null ? "The stream is null." : "The stream is empty.";
                        _logger.LogWarning("Failed to generate PDF for \\\"{PdfName}\\\". {Error}", pdfName, error);
                        return BadRequest(ApiError.FromMessage($"Failed to generate PDF. {error}"));
                    }

                    var edmsResult = await _edmsClient.UploadDocument(
                        businessGroup,
                        pdfName,
                        stream);
                    if (edmsResult.IsLeft)
                    {
                        _logger.LogWarning($"Failed to upload \"{pdfName}\". Error: {edmsResult.Left().Message}");
                        return BadRequest(ApiError.FromMessage(edmsResult.Left().Message));
                    }
                    var document = new UploadedDocument(bereavementReferenceNumber.ToString(), businessGroup, "bereavement", null, pdfName, edmsResult.Right().Uuid, DocumentSource.Outgoing, true, "NODEA");
                    var allDocumentsCopy = documents.ToList();
                    allDocumentsCopy.Add(document);
                    var errorOrDocuments = await _edmsDocumentsIndexing.PostIndexBereavementDocuments(businessGroup, errorOrCaseNumber.Right(), allDocumentsCopy);
                    if (errorOrDocuments.IsLeft)
                    {
                        _logger.LogWarning($"Failed to postindex documents. Error: {errorOrDocuments.Left().Message}.");
                        return BadRequest(ApiError.FromMessage(errorOrDocuments.Left().Message));
                    }

                    _journeyDocumentsRepository.RemoveAll(documents);
                    await _mdpUnitOfWork.Commit();
                    await _bereavementUnitOfWork.Commit();
                    await _uow.Commit();

                    await _smtpClient.Send(
                        request.Reporter.Email,
                        template.EmailFrom,
                        htmlBody,
                        template.EmailSubject);

                    journey.UpdateExpiryDate(DateTimeOffset.UtcNow.AddMinutes(_journeyConfiguration.ValidityPeriodInMin));
                    await _bereavementUnitOfWork.Commit();

                    return Ok(BereavementJourneySubmitResponse.From(errorOrCaseNumber.Right()));
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("address/search")]
    [ProducesResponseType(typeof(BereavementAddressSummariesResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> FindAddressSummary([FromQuery] BereavementAddressSummaryRequest request)
    {
        return (await _loqateApiClient.Find(request.Text, request.Container, request.Language, request.Countries))
            .Match<IActionResult>(response => Ok(response.Items.Select(BereavementAddressSummariesResponse.From)),
                error => BadRequest(ApiError.FromMessage(error.Message)));
    }

    [HttpGet("address/{addressId}")]
    [ProducesResponseType(typeof(BereavementAddressResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> Get(string addressId)
    {
        return (await _loqateApiClient.GetDetails(addressId))
            .Match<IActionResult>(response => Ok(response.Items.Select(BereavementAddressResponse.From)),
                error => BadRequest(ApiError.FromMessage(error.Message)));
    }
}