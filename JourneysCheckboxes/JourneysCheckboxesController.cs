using System;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Infrastructure.BereavementDb;
using WTW.MdpService.Infrastructure.Journeys;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.JourneysCheckboxes;

[ApiController]
public class JourneysCheckboxesController : ControllerBase
{
    private readonly IJourneyService _journeyService;
    private readonly IMdpUnitOfWork _mdpUnitOfWork;
    private readonly IBereavementUnitOfWork _bereavementUnitOfWork;
    private readonly ILogger<JourneysCheckboxesController> _logger;

    public JourneysCheckboxesController(
        IJourneyService journeyService,
        IMdpUnitOfWork mdpUnitOfWork,
        IBereavementUnitOfWork bereavementUnitOfWork,
        ILogger<JourneysCheckboxesController> logger)
    {
        _journeyService = journeyService;
        _mdpUnitOfWork = mdpUnitOfWork;
        _bereavementUnitOfWork = bereavementUnitOfWork;
        _logger = logger;
    }

    [HttpPost("api/journeys/{journeyType}/page/{pageKey}/checkboxesLists")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> SaveCheckboxes(string journeyType, string pageKey, [FromBody] SaveJourneyCheckboxesRequest request)
    {
        var (_, referenceNumber, businessGroup) = HttpContext.User.BereavementUserOrMember();
        var journey = await _journeyService.GetJourney(journeyType, businessGroup, referenceNumber);
        if (journey.IsNone)
        {
            _logger.LogError("Journey {journeyType} not found for user with reference number: {referenceNumber} and businessGroup {businessGroup}.", journeyType, referenceNumber, businessGroup);
            return NotFound(ApiError.FromMessage("Journey is not started yet."));
        }
            
        var step = journey.Value().GetStepByKey(pageKey);
        if (step.IsSome)
        {
            step.Value().AddCheckboxesList(new CheckboxesList(request.CheckboxesListKey, request.Checkboxes.Select(x => (x.Key, x.AnswerValue))));
            await _mdpUnitOfWork.Commit();
            await _bereavementUnitOfWork.Commit();
            return NoContent();
        }

        var newStepCreatedOrError = journey.Value().TrySubmitStep(pageKey, "next_page_is_not_set", DateTimeOffset.UtcNow);
        if (newStepCreatedOrError.IsLeft)
        {
            _logger.LogError("Error during creation new step. Error {error}", newStepCreatedOrError.Left().Message);
            return BadRequest(ApiError.FromMessage(newStepCreatedOrError.Left().Message));
        }

        var newStep = journey.Value().GetStepByKey(pageKey);
        newStep.Value().AddCheckboxesList(new CheckboxesList(request.CheckboxesListKey, request.Checkboxes.Select(x => (x.Key, x.AnswerValue))));


        await _mdpUnitOfWork.Commit();
        await _bereavementUnitOfWork.Commit();
        return NoContent();
    }

    [HttpGet("api/journeys/{journeyType}/page/{pageKey}/checkboxesLists/{checkboxesListKey}")]
    [ProducesResponseType(typeof(JourneyCheckboxesResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> Checkboxes(string journeyType, string pageKey, string checkboxesListKey)
    {
        var (_, referenceNumber, businessGroup) = HttpContext.User.BereavementUserOrMember();
        var journey = await _journeyService.GetJourney(journeyType, businessGroup, referenceNumber);
        if (journey.IsNone)
        {
            _logger.LogError("Journey {journeyType} not found for user with reference number: {referenceNumber} and businessGroup {businessGroup}.", journeyType, referenceNumber, businessGroup);
            return NotFound(ApiError.NotFound());
        }
            
        var step = journey.Value().GetStepByKey(pageKey);
        if (step.IsNone)
        {
            if (journey.Value().FindStepFromNextPageKeys(new[] { pageKey }).IsSome)
            {
                return Ok(new JourneyCheckboxesResponse(
                    new CheckboxesList(
                        checkboxesListKey,
                        Array.Empty<(string Key, bool Value)>())));
            }

            _logger.LogError("Journey {journeyType} does not contain step with current page key: {pageKey}.", journeyType, pageKey);
            return NotFound(ApiError.NotFound());
        }
            
        var checkBoxesList = step.Value().GetCheckboxesListByKey(checkboxesListKey);
        if (checkBoxesList.IsNone)
        {
            _logger.LogError("Journey {journeyType} does not contain checkboxes list with current checkboxes list key: {checkboxesListKey}.", journeyType, checkboxesListKey);
            return NotFound(ApiError.NotFound());
        }
            
        return Ok(new JourneyCheckboxesResponse(checkBoxesList.Value()));
    }
}