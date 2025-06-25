using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Infrastructure.Journeys;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Journeys.JourneysGenericData;

public class JourneysGenericDataController : ControllerBase
{
    private readonly IMdpUnitOfWork _mdpUnitOfWork;
    private readonly ILogger<JourneysGenericDataController> _logger;
    private readonly IJourneyService _journeyService;

    public JourneysGenericDataController(IMdpUnitOfWork mdpUnitOfWork,
        ILogger<JourneysGenericDataController> logger,
        IJourneyService journeyService)
    {
        _mdpUnitOfWork = mdpUnitOfWork;
        _logger = logger;
        _journeyService = journeyService;
    }

    [HttpPost("api/journeys/{journeyType}/page/{pageKey}/stepData/{formKey}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> SaveGenericData(string journeyType, string pageKey, string formKey, [FromBody] SaveJourneyGenericDataRequest request)
    {
        var (_, referenceNumber, businessGroup) = HttpContext.User.BereavementUserOrMember();
        var journey = await _journeyService.GetJourney(journeyType, businessGroup, referenceNumber);
        if (journey.IsNone)
        {
            _logger.LogWarning("{journeyType} journey is not started yet.", journeyType);
            return NotFound(ApiError.FromMessage($"{journeyType} journey is not started yet."));
        }

        var step = journey.Value().GetStepByKey(pageKey);
        if (step.IsSome)
        {
            step.Value().UpdateGenericData(formKey, request.GenericDataJson);
            await _mdpUnitOfWork.Commit();
            return NoContent();
        }

        var newStepCreatedOrError = journey.Value().TrySubmitStep(pageKey, "next_page_is_not_set", DateTimeOffset.UtcNow);
        if (newStepCreatedOrError.IsLeft)
        {
            _logger.LogError("Error during creation new step. Error {error}", newStepCreatedOrError.Left().Message);
            return BadRequest(ApiError.FromMessage(newStepCreatedOrError.Left().Message));
        }

        var newStep = journey.Value().GetStepByKey(pageKey);
        newStep.Value().UpdateGenericData(formKey, request.GenericDataJson);

        await _mdpUnitOfWork.Commit();
        return NoContent();
    }

    [HttpGet("api/journeys/{journeyType}/page/{pageKey}/stepData/{formKey}")]
    [ProducesResponseType(typeof(JourneyGenericDataResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GetGenericData(string journeyType, string pageKey, string formKey)
    {
        var (_, referenceNumber, businessGroup) = HttpContext.User.BereavementUserOrMember();
        var journey = await _journeyService.GetJourney(journeyType, businessGroup, referenceNumber);
        if (journey.IsNone)
            return NotFound(ApiError.NotFound());

        var step = journey.Value().GetStepByKey(pageKey);
        if (step.IsNone)
        {
            if (journey.Value().FindStepFromNextPageKeys(new[] { pageKey }).IsSome)
            {
                return Ok(new JourneyGenericDataResponse(new JourneyGenericData(string.Empty, formKey)));
            }

            _logger.LogWarning("Journey does not contain step with current page key: \'{pageKey}\'.", pageKey);
            return NotFound(ApiError.NotFound());
        }

        var genericData = step.Value().GetGenericDataByKey(formKey);
        if (genericData.IsNone)
        {
            _logger.LogWarning("Journey does not contain generic data with current form key: \'{formKey}\'.", formKey);
            return NotFound(ApiError.NotFound());
        }

        return Ok(new JourneyGenericDataResponse(genericData.Value()));
    }
}