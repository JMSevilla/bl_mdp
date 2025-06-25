using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.QuoteSelectionJourneys;

[ApiController]
[Route("api/quote-selection-journey")]
public class QuoteSelectionJourneyController : ControllerBase
{
    private readonly MdpUnitOfWork _mdpDbUnitOfWork;
    private readonly IQuoteSelectionJourneyRepository _quoteSelectionJourneyRepository;

    public QuoteSelectionJourneyController(MdpUnitOfWork mdpDbUnitOfWork,
        IQuoteSelectionJourneyRepository quoteSelectionJourneyRepository)
    {
        _mdpDbUnitOfWork = mdpDbUnitOfWork;
        _quoteSelectionJourneyRepository = quoteSelectionJourneyRepository;
    }

    [HttpPost("submit-selection-step")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SubmitQuoteSelectionStep([FromBody] SubmitQuoteSelectionStepRequest request)
    {
        var now = DateTimeOffset.UtcNow;
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        return await _quoteSelectionJourneyRepository.Find(businessGroup, referenceNumber)
                .ToAsync()
                .MatchAsync<IActionResult>(async journey =>
                {
                    if (!request.SelectedQuoteName.Contains('.'))
                    {
                        await using var transaction = await _mdpDbUnitOfWork.BeginTransactionAsync();
                        try
                        {
                            _quoteSelectionJourneyRepository.Remove(journey);
                            await _mdpDbUnitOfWork.Commit();

                            var newJourney = new QuoteSelectionJourney(businessGroup, referenceNumber, now, request.CurrentPageKey, request.NextPageKey, request.SelectedQuoteName);
                            await _quoteSelectionJourneyRepository.Add(newJourney);
                            await _mdpDbUnitOfWork.Commit();

                            await transaction.CommitAsync();
                        }
                        catch (Exception)
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }

                        return NoContent();
                    }

                    var result = journey.TrySubmitStep(
                            request.CurrentPageKey,
                            request.NextPageKey,
                            now,
                            QuoteSelectionJourney.QuestionKey,
                            request.SelectedQuoteName);

                    if (result.IsLeft)
                        return BadRequest(ApiError.FromMessage(result.Left().Message));

                    await _mdpDbUnitOfWork.Commit();
                    return NoContent();
                },
                async () =>
                {
                    var journey = new QuoteSelectionJourney(businessGroup, referenceNumber, now, request.CurrentPageKey, request.NextPageKey, request.SelectedQuoteName);
                    await _quoteSelectionJourneyRepository.Add(journey);
                    await _mdpDbUnitOfWork.Commit();

                    return NoContent();
                });
    }

    [HttpGet("previous-step/{currentPageKey}")]
    [ProducesResponseType(typeof(QuoteSelectionPreviousStepResponse), 200)]
    [SwaggerResponse(404, "Not Found. Available error codes: previous_page_key_not_found", typeof(ApiError))]
    public async Task<IActionResult> PreviousStep([MinLength(2), MaxLength(25)] string currentPageKey)
    {
        return (await _quoteSelectionJourneyRepository.Find(
            HttpContext.User.User().BusinessGroup,
            HttpContext.User.User().ReferenceNumber))
                .Match<IActionResult>(journey =>
                {
                    var result = journey.PreviousStep(currentPageKey);
                    if (result.IsNone)
                        return NotFound(ApiError.From(
                        "Previous step does not exist in Quote selection journey for given current page key.",
                        "previous_page_key_not_found"));

                    return Ok(QuoteSelectionPreviousStepResponse.From(result.Value()));
                },
                () => NotFound(ApiError.From(
                    "Quote selection journey does not exist for given member",
                    "previous_page_key_not_found")));
    }

    [HttpGet("question-form/{currentPageKey}")]
    [ProducesResponseType(typeof(QuoteSelectionQuestionFormResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> QuestionForm([MinLength(2), MaxLength(25)] string currentPageKey)
    {
        return (await _quoteSelectionJourneyRepository.Find(
            HttpContext.User.User().BusinessGroup,
            HttpContext.User.User().ReferenceNumber))
                .Match<IActionResult>(journey =>
                {
                    var result = journey.QuestionForm(currentPageKey);

                    if (result.IsNone)
                        return NotFound(ApiError.NotFound());

                    return Ok(QuoteSelectionQuestionFormResponse.From(result.Value()));
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("integrity/{pageKey}")]
    [ProducesResponseType(typeof(QuoteSelectionIntegrityResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> CheckJourneyIntegrity([MinLength(2), MaxLength(25)] string pageKey)
    {
        return (await _quoteSelectionJourneyRepository.Find(
            HttpContext.User.User().BusinessGroup,
            HttpContext.User.User().ReferenceNumber))
                .Match<IActionResult>(
                    journey => Ok(QuoteSelectionIntegrityResponse.From(journey.GetRedirectStepPageKey(pageKey))),
                    () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("selections")]
    [ProducesResponseType(typeof(QuoteSelectionsResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> QuoteSelections()
    {
        return (await _quoteSelectionJourneyRepository.Find(HttpContext.User.User().BusinessGroup, HttpContext.User.User().ReferenceNumber))
                .Match<IActionResult>(journey =>
                {
                    var result = journey.QuoteSelection();

                    if (result.IsNone)
                        return BadRequest(ApiError.NotFound());

                    return Ok(QuoteSelectionsResponse.From(result.Value()));
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpPost("clear-selections")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> ClearQuoteSelections()
    {
        return await _quoteSelectionJourneyRepository.Find(HttpContext.User.User().BusinessGroup, HttpContext.User.User().ReferenceNumber)
                .ToAsync()
                .MatchAsync<IActionResult>(async journey =>
                {
                    _quoteSelectionJourneyRepository.Remove(journey);
                    await _mdpDbUnitOfWork.Commit();
                    return NoContent();
                },
                () => NotFound(ApiError.NotFound()));
    }
}