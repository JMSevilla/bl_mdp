using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.IdentityVerification.Services;
using WTW.MdpService.Infrastructure.EmailConfirmation;
using WTW.MdpService.Journeys.Submit.Services;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.Extensions;
using WTW.Web.LanguageExt;
using static WTW.Web.MdpConstants;

namespace WTW.MdpService.Journeys.Submit;

[ApiController]
public class JourneySubmitController : ControllerBase
{
    private readonly ICaseRequestFactory _caseApiRequestModelFactory;
    private readonly ICaseService _caseService;
    private readonly IDocumentRenderer _documentRenderer;
    private readonly IDocumentsUploaderService _documentUploader;
    private readonly IEmailConfirmationSmtpClient _smtpClient;
    private readonly IGenericJourneyService _genericJourneyService;
    private readonly IDocumentsRendererDataFactory _documentsRendererDataFactory;
    private readonly ILogger<JourneySubmitController> _logger;
    private readonly IIdvService _idvService;

    public JourneySubmitController(
        ICaseRequestFactory caseApiRequestModelFactory,
        ICaseService caseService,
        IDocumentRenderer documentRenderer,
        IDocumentsUploaderService documentUploader,
        IEmailConfirmationSmtpClient smtpClient,
        IGenericJourneyService genericJourneyService,
        IDocumentsRendererDataFactory documentsRendererDataFactory,
        ILogger<JourneySubmitController> logger,
        IIdvService idvService)
    {
        _caseApiRequestModelFactory = caseApiRequestModelFactory;
        _caseService = caseService;
        _documentRenderer = documentRenderer;
        _documentUploader = documentUploader;
        _smtpClient = smtpClient;
        _genericJourneyService = genericJourneyService;
        _documentsRendererDataFactory = documentsRendererDataFactory;
        _logger = logger;
        _idvService = idvService;
    }

    [HttpPost("api/journeys/submit-case")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SubmitQuoteRequestCase([FromBody] SubmitQuoteRequestCaseRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        _logger.LogInformation($"Case Type: {request.CaseType}, BusinessGroup: {businessGroup}, ReferenceNumber: {referenceNumber}");

        var documentsRendererData = _documentsRendererDataFactory.CreateForQuoteRequest(businessGroup, referenceNumber, "TEMP_CASE_NUMBER", request.AccessKey, request.CaseType);
        var document = await _documentRenderer.RenderGenericSummaryPdf(
            documentsRendererData,
            Request.Headers[HeaderNames.Authorization],
            Request.Headers["env"]);

        var requestModelOrError = await _caseApiRequestModelFactory.CreateForQuoteRequest(businessGroup, referenceNumber, request.CaseType);
        if (requestModelOrError.IsLeft)
        {
            _logger.LogError("Failed to create Quote request case api request model. Error: {error}", requestModelOrError.Left().Message);
            return BadRequest(ApiError.FromMessage(requestModelOrError.Left().Message));
        }

        var errorOrCaseNumber = await _caseService.Create(requestModelOrError.Right());
        if (errorOrCaseNumber.IsLeft)
        {
            _logger.LogError("Failed to create Quote request case. Error: {error}", errorOrCaseNumber.Left().Message);
            return BadRequest(ApiError.FromMessage("Failed to create case."));
        }

        var error = await _documentUploader.UploadQuoteRequestSummary(businessGroup, referenceNumber, errorOrCaseNumber.Right(), request.CaseType, document.PdfStream, document.FileName);
        if (error.HasValue)
        {
            _logger.LogError("Failed to upload quote request submit document. Error: {error}", error.Value.Message);
            return BadRequest(ApiError.FromMessage(error.Value.Message));
        }

        try
        {
            var summaryEmailDetails = await _documentRenderer.RenderGenericJourneySummaryEmail(
                documentsRendererData,
                Request.Headers[HeaderNames.Authorization],
                Request.Headers["env"]);

            await _smtpClient.Send(
                summaryEmailDetails.EmailTo,
                summaryEmailDetails.EmailFrom,
                summaryEmailDetails.EmailHtmlBody,
                summaryEmailDetails.EmailSubject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send quote request submit email. Case type: {request.CaseType}.");
        }

        return NoContent();
    }

    [HttpPost("api/journeys/{journeyType}/submit")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SubmitJourney([FromBody] SubmitDcJourneyRequest request, string journeyType)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        _logger.LogInformation("Journey Type: {journeyType}, BusinessGroup: {businessGroup}, ReferenceNumber: {referenceNumber}",
            journeyType, businessGroup, referenceNumber);

        if (!await _genericJourneyService.ExistsJourney(businessGroup, referenceNumber, journeyType))
            return NotFound(ApiError.FromMessage($"Journey with type \"{journeyType}\" not found."));

        var caseCode = CaseCodes.RTP9;
        if (!string.IsNullOrWhiteSpace(request.ContentAccessKey) && request.ContentAccessKey.IsSelectedQuoteIncomeDrawdownC2STFCL())
            caseCode = CaseCodes.TOP9;

        var requestModel = _caseApiRequestModelFactory.CreateForGenericRetirement(businessGroup, referenceNumber, caseCode);
        var errorOrCaseNumber = await _caseService.Create(requestModel);
        if (errorOrCaseNumber.IsLeft)
        {
            _logger.LogError("Failed to create submit case. Journey type: {journeyType}. Error: {error}", journeyType, errorOrCaseNumber.Left().Message);
            return BadRequest(ApiError.FromMessage("Failed to create case."));
        }

        var errorOrIdvSaved = await _idvService.SaveIdentityVerification(businessGroup, referenceNumber, caseCode, errorOrCaseNumber.Right());
        if (errorOrIdvSaved.IsLeft)
        {
            _logger.LogWarning("Failed to save IDV results.Error: {error}", errorOrIdvSaved.Left().Message);
        }

        var documentsRendererData = _documentsRendererDataFactory.CreateForSubmit(journeyType, businessGroup, referenceNumber, request.ContentAccessKey, errorOrCaseNumber.Right());
        var document = await _documentRenderer.RenderGenericSummaryPdf(
            documentsRendererData,
            Request.Headers[HeaderNames.Authorization],
            Request.Headers["env"]);

        var errorOrImageId = await _documentUploader.UploadGenericRetirementDocuments(businessGroup, referenceNumber, errorOrCaseNumber.Right(), journeyType, document.PdfStream, document.FileName);
        if (errorOrImageId.IsLeft)
        {
            _logger.LogError("Failed to upload retirement submit documents.Journey type: {journeyType}. Error: {error}", journeyType, errorOrImageId.Left().Message);
            return BadRequest(ApiError.FromMessage(errorOrImageId.Left().Message));
        }

        await _genericJourneyService.SetStatusSubmitted(businessGroup, referenceNumber, journeyType);
        await _genericJourneyService.SaveSubmissionDetailsToGenericData(businessGroup, referenceNumber, journeyType, (errorOrCaseNumber.Right(), errorOrImageId.Right()));

        try
        {
            var summaryEmailDetails = await _documentRenderer.RenderGenericJourneySummaryEmail(
                documentsRendererData,
                Request.Headers[HeaderNames.Authorization],
                Request.Headers["env"]);

            await _smtpClient.Send(
                summaryEmailDetails.EmailTo,
                summaryEmailDetails.EmailFrom,
                summaryEmailDetails.EmailHtmlBody,
                summaryEmailDetails.EmailSubject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send retirement submit email. Journey type: {journeyType}.", journeyType);
        }

        return NoContent();
    }
}