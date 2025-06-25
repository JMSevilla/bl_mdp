using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.Journeys.Documents;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.Templates.RetirementApplication.Submit;
using WTW.Web;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.RetirementJourneys;

[ApiController]
[Route("api/retirement-journey")]
public class RetirementJourneyController : ControllerBase
{
    private readonly IRetirementJourneyRepository _repository;
    private readonly IMdpUnitOfWork _mdpDbUnitOfWork;
    private readonly IContentClient _contentClient;
    private readonly IRetirementApplicationQuotesV2 _retirementApplicationQuotes;
    private readonly ICalculationsRepository _calculationsRepository;
    private readonly IJourneyDocumentsHandlerService _journeyDocumentsHandlerService;
    private readonly ILogger<RetirementJourneyController> _logger;

    public RetirementJourneyController(
        IRetirementJourneyRepository repository,
        IMdpUnitOfWork mdpDbUnitOfWork,
        IContentClient contentClient,
        IRetirementApplicationQuotesV2 retirementApplicationQuotes,
        ICalculationsRepository calculationsRepository,
        IJourneyDocumentsHandlerService journeyDocumentsHandlerService,
        ILogger<RetirementJourneyController> logger)
    {
        _repository = repository;
        _mdpDbUnitOfWork = mdpDbUnitOfWork;
        _contentClient = contentClient;
        _retirementApplicationQuotes = retirementApplicationQuotes;
        _calculationsRepository = calculationsRepository;
        _journeyDocumentsHandlerService = journeyDocumentsHandlerService;
        _logger = logger;
    }

    [HttpGet("previous-step/{currentPageKey}")]
    [ProducesResponseType(typeof(PreviousStepResponse), 200)]
    [SwaggerResponse(404, "Not Found. Available error codes: previous_page_key_not_found", typeof(ApiError))]
    public async Task<IActionResult> PreviousStep([MinLength(2), MaxLength(25)] string currentPageKey)
    {
        return (await _repository.FindUnexpiredJourney(
                HttpContext.User.User().BusinessGroup,
                HttpContext.User.User().ReferenceNumber,
                DateTimeOffset.UtcNow))
            .Match<IActionResult>(journey =>
                {
                    var result = journey.PreviousStep(currentPageKey);
                    if (result.IsNone)
                        return NotFound(ApiError.From(
                            "Previous step does not exist in retiremnt journey for given current page key.",
                            "previous_page_key_not_found"));

                    return Ok(PreviousStepResponse.From(result.Value()));
                },
                () => NotFound(ApiError.From(
                            "Retirement journey does not exist for given member",
                            "previous_page_key_not_found")));
    }

    [HttpPost("switch-step")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SwitchStep([FromBody] SwitchStepRequest request)
    {
        var now = DateTimeOffset.UtcNow;
        return await _repository.FindUnexpiredJourney(HttpContext.User.User().BusinessGroup, HttpContext.User.User().ReferenceNumber,
                now)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    var result = journey.UpdateStep(request.SwitchPageKey, request.NextPageKey);

                    if (result.HasValue)
                        return NotFound(ApiError.From(
                            result.Value.Message,
                            "previous_page_key_not_found"));


                    await _mdpDbUnitOfWork.Commit();
                    return NoContent();
                },
               () => BadRequest(ApiError.FromMessage("Member did not start retirement journey yet")));
    }

    [HttpPost("submit-step")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SubmitStep([FromBody] SubmitStepRequest request)
    {
        var now = DateTimeOffset.UtcNow;
        return await _repository.FindUnexpiredJourney(HttpContext.User.User().BusinessGroup, HttpContext.User.User().ReferenceNumber,
                now)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    var result = journey.TrySubmitStep(
                            request.CurrentPageKey,
                            request.NextPageKey,
                            now);

                    if (result.IsLeft)
                        return BadRequest(ApiError.FromMessage(result.Left().Message));

                    await _mdpDbUnitOfWork.Commit();
                    return NoContent();
                },
                () => BadRequest(ApiError.FromMessage("Member did not start retirement journey yet")));
    }

    [HttpPost("submit-question-step")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SubmitQuestionStep([FromBody] SubmitQuestionStepRequest request)
    {
        var now = DateTimeOffset.UtcNow;
        return await _repository.FindUnexpiredJourney(HttpContext.User.User().BusinessGroup, HttpContext.User.User().ReferenceNumber,
            now)
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
                        return BadRequest(ApiError.FromMessage(result.Left().Message));

                    await _mdpDbUnitOfWork.Commit();
                    return NoContent();
                },
                () => BadRequest(ApiError.FromMessage("Member did not start retirement journey yet")));
    }

    [HttpPost("submit-financial-advise")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SubmitFinancialAdviseDate([FromBody] SubmitFinancialAdviseRequest request)
    {
        var now = DateTimeOffset.UtcNow;
        return await _repository.FindUnexpiredJourney(
                HttpContext.User.User().BusinessGroup,
                HttpContext.User.User().ReferenceNumber,
                now)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    var result = journey.SetFinancialAdviseDate(request.FinancialAdviseDate.Value);

                    if (result.HasValue)
                        return BadRequest(ApiError.FromMessage(result.Value.Message));

                    await _mdpDbUnitOfWork.Commit();
                    return NoContent();
                },
                () => BadRequest(ApiError.FromMessage("Member did not start retirement journey yet")));
    }

    [HttpPost("submit-pension-wise")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SubmitPensionWiseDate([FromBody] SubmitPensionWiseRequest request)
    {
        var now = DateTimeOffset.UtcNow;
        return await _repository.FindUnexpiredJourney(
                HttpContext.User.User().BusinessGroup,
                HttpContext.User.User().ReferenceNumber,
                now)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    var result = journey.SetPensionWiseDate(request.PensionWiseDate.Value);

                    if (result.HasValue)
                        return BadRequest(ApiError.FromMessage(result.Value.Message));

                    await _mdpDbUnitOfWork.Commit();
                    return NoContent();
                },
                () => BadRequest(ApiError.FromMessage("Member did not start retirement journey yet")));
    }

    [HttpGet("financial-advise")]
    [ProducesResponseType(typeof(FinancialAdviseResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> FinancialAdvise()
    {
        return (await _repository.FindUnexpiredJourney(HttpContext.User.User().BusinessGroup,
                HttpContext.User.User().ReferenceNumber, DateTimeOffset.UtcNow))
            .Match<IActionResult>(
                journey => Ok(FinancialAdviseResponse.From(journey)),
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("pension-wise")]
    [ProducesResponseType(typeof(FinancialAdviseResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> PensionWise()
    {
        return (await _repository.FindUnexpiredJourney(HttpContext.User.User().BusinessGroup,
                HttpContext.User.User().ReferenceNumber, DateTimeOffset.UtcNow))
            .Match<IActionResult>(
                journey => Ok(PensionWiseResponse.From(journey)),
                () => NotFound(ApiError.NotFound()));
    }

    [HttpPost("submit-lifetime-allowance")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SubmitLifetimeAllowance([FromBody] SubmitLifetimeAllowanceRequest request)
    {
        var now = DateTimeOffset.UtcNow;
        return await _repository.FindUnexpiredJourney(
                HttpContext.User.User().BusinessGroup,
                HttpContext.User.User().ReferenceNumber,
                now)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    var result = journey.SetEnteredLtaPercentage(request.Percentage);

                    if (result.IsLeft)
                        return BadRequest(ApiError.FromMessage(result.Left().Message));

                    await _mdpDbUnitOfWork.Commit();
                    return NoContent();
                },
                () => BadRequest(ApiError.FromMessage("Member did not start retirement journey yet")));
    }


    [HttpPost("submit-opt-out-pension-wise")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SubmitOptOutPensionWise([FromBody] SubmitOptOutPensionWiseRequest request)
    {
        var now = DateTimeOffset.UtcNow;
        return await _repository.FindUnexpiredJourney(
                HttpContext.User.User().BusinessGroup,
                HttpContext.User.User().ReferenceNumber,
                now)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    journey.SetOptOutPensionWise(request.OptOutPensionWise);

                    await _mdpDbUnitOfWork.Commit();
                    return NoContent();
                },
                () => BadRequest(ApiError.FromMessage("Member did not start retirement journey yet")));
    }

    [HttpGet("pension-wise-opt-out")]
    [ProducesResponseType(typeof(OptOutPensionWiseResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> PensionWiseOptOut()
    {
        return (await _repository.FindUnexpiredJourney(HttpContext.User.User().BusinessGroup, HttpContext.User.User().ReferenceNumber, DateTimeOffset.UtcNow))
            .Match<IActionResult>(
                journey => Ok(OptOutPensionWiseResponse.From(journey)),
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("question-form/{currentPageKey}")]
    [ProducesResponseType(typeof(QuestionFormResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> QuestionForm([MinLength(2), MaxLength(25)] string currentPageKey)
    {
        return (await _repository.FindUnexpiredJourney(HttpContext.User.User().BusinessGroup, HttpContext.User.User().ReferenceNumber, DateTimeOffset.UtcNow))
            .Match<IActionResult>(
                journey =>
                {
                    var result = journey.QuestionForm(currentPageKey);

                    if (result.IsNone)
                        return NotFound(ApiError.NotFound());

                    return Ok(QuestionFormResponse.From(result.Value()));
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("lifetime-allowance")]
    [ProducesResponseType(typeof(LifetimeAllowanceResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> LifetimeAllowance()
    {
        return (await _repository.FindUnexpiredJourney(HttpContext.User.User().BusinessGroup, HttpContext.User.User().ReferenceNumber, DateTimeOffset.UtcNow))
            .Match<IActionResult>(
                journey => Ok(LifetimeAllowanceResponse.From(journey)),
                () => NotFound(ApiError.NotFound()));
    }

    [HttpDelete]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> DeleteJourney()
    {
        return await _repository
            .Find(HttpContext.User.User().BusinessGroup, HttpContext.User.User().ReferenceNumber)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async journey =>
                {
                    _repository.Remove(journey);
                    _calculationsRepository.Remove(journey.Calculation);
                    await _mdpDbUnitOfWork.Commit();

                    return NoContent();
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("answers")]
    [ProducesResponseType(typeof(IEnumerable<QuestionFormResponse>), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GetJourneyAnswers([FromQuery] string[] questionKeys)
    {
        return (await _repository.FindUnexpiredJourney(HttpContext.User.User().BusinessGroup, HttpContext.User.User().ReferenceNumber, DateTimeOffset.UtcNow))
            .Match<IActionResult>(
                journey => Ok(journey.QuestionForms(questionKeys).Select(QuestionFormResponse.From)),
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("integrity/{pageKey}")]
    [ProducesResponseType(typeof(IntegrityResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> CheckJourneyIntegrity([MinLength(2), MaxLength(25)] string pageKey)
    {
        return await _repository.FindUnexpiredJourney(HttpContext.User.User().BusinessGroup, HttpContext.User.User().ReferenceNumber, DateTimeOffset.UtcNow)
            .ToAsync()
            .MatchAsync<IActionResult>(
               async journey =>
               {
                   journey.UpdateOldSeqNumbers();
                   await _mdpDbUnitOfWork.Commit();
                   return Ok(IntegrityResponse.From(journey.GetRedirectStepPageKey(pageKey)));
               },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("retirement-application")]
    [ProducesResponseType(typeof(RetirementApplicationResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> RetirementApplicationData([FromQuery] string contentAccessKey)
    {
        var now = DateTimeOffset.UtcNow;
        return await _repository.FindUnexpiredOrSubmittedJourney(
                HttpContext.User.User().BusinessGroup,
                HttpContext.User.User().ReferenceNumber,
                now)
            .ToAsync()
        .MatchAsync<IActionResult>(
        async journey =>
        {
            var retirementOptions = await _contentClient.FindRetirementOptions(journey.MemberQuote.Label, contentAccessKey);
            var retirementSummary = await _retirementApplicationQuotes.GetSummaryFigures(journey, retirementOptions);

            return Ok(RetirementApplicationResponse.From(
                journey.MemberQuote,
                journey.ExpirationDate,
                journey.MemberQuote.SearchedRetirementDate,
                journey.SubmissionDate,
                journey.Status(now),
                retirementSummary,
                journey.GetJourneyGenericDataList()));
        },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("download/summary-pdf")]
    [ProducesResponseType(typeof(FileResult), 200, "application/octet-stream")]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> DownloadSummaryPdf()
    {
        return (await _repository
            .FindUnexpiredOrSubmittedJourney(
                HttpContext.User.User().BusinessGroup,
                HttpContext.User.User().ReferenceNumber,
                DateTimeOffset.UtcNow))
            .Match<IActionResult>(
                journey =>
                {
                    if (journey.SummaryPdf.ToOption().IsNone)
                        return NotFound(ApiError.NotFound());

                    return File(journey.SummaryPdf, "application/pdf", "retirement-journey-summary.pdf");
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpPost("gbg/{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> SaveGbgId(Guid id)
    {
        return await _repository
            .FindUnexpiredJourney(
                HttpContext.User.User().BusinessGroup,
                HttpContext.User.User().ReferenceNumber,
                DateTimeOffset.UtcNow)
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

    [HttpPost("documents/post-submission")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> AddPostSubmissionDocuments()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        return await _repository.Find(businessGroup, referenceNumber)
             .ToAsync()
             .MatchAsync<IActionResult>(
                async journey =>
                {
                    var result = await _journeyDocumentsHandlerService.PostIndex(businessGroup, referenceNumber, journey.CaseNumber, MdpConstants.JourneyTypeRetirement);
                    if (result.IsLeft)
                        return BadRequest(ApiError.FromMessage(result.Left().Message));

                    await _mdpDbUnitOfWork.Commit();
                    return NoContent();
                },
                () => NotFound(ApiError.NotFound()));
    }
}