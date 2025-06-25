using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.MdpService.Journeys.Submit.Services;
using WTW.MdpService.Retirement;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.Extensions;
using WTW.Web.LanguageExt;
using WTW.Web.Serialization;

namespace WTW.MdpService.Journeys.Start;

[ApiController]
public class GenericJourneysStartController : ControllerBase
{
    private readonly IJourneysRepository _journeysRepository;
    private readonly ICalculationsRepository _calculationsRepository;
    private readonly IMdpUnitOfWork _mdpUnitOfWork;
    private readonly IJsonConversionService _jsonConversionService;
    private readonly ICalculationsParser _calculationsParser;
    private readonly ILogger<GenericJourneysStartController> _logger;
    private readonly IRetirementService _retirementService;
    private readonly IGenericJourneyService _genericJourneyService;
    private readonly IMemberRepository _memberRepository;

    public GenericJourneysStartController(IJourneysRepository journeysRepository,
        ICalculationsRepository calculationsRepository,
        IMdpUnitOfWork mdpUnitOfWork,
        IJsonConversionService jsonConversionService,
        ICalculationsParser calculationsParser,
        ILogger<GenericJourneysStartController> logger,
        IRetirementService retirementService,
        IGenericJourneyService genericJourneyService, IMemberRepository memberRepository)
    {
        _journeysRepository = journeysRepository;
        _calculationsRepository = calculationsRepository;
        _mdpUnitOfWork = mdpUnitOfWork;
        _jsonConversionService = jsonConversionService;
        _calculationsParser = calculationsParser;
        _logger = logger;
        _retirementService = retirementService;
        _genericJourneyService = genericJourneyService;
        _memberRepository = memberRepository;
    }

    [HttpPost("api/journeys/{journeyType}/start-dc-journey")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> StartDcJourney(string journeyType, [FromBody] StartDcJourneyRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        return await _journeysRepository.FindUnexpired(businessGroup, referenceNumber, journeyType)
            .ToAsync()
            .MatchAsync<IActionResult>(
            _ => NoContent(),
            async () =>
            {
                var journeyToRemove = await _journeysRepository.Find(businessGroup, referenceNumber, journeyType);
                if (journeyToRemove.IsSome)
                    _journeysRepository.Remove(journeyToRemove.Value());

                var calculation = await _calculationsRepository.Find(referenceNumber, businessGroup);
                if (calculation.IsNone)
                {
                    _logger.LogWarning("Calculation must be present for member attempting to start dc journey.");
                    return BadRequest(ApiError.FromMessage("Calculation does not exist."));
                }

                var dcExploreOptionsData = await _journeysRepository.Find(businessGroup, referenceNumber, "dcexploreoptions");
                var retirementV2 = _calculationsParser.GetRetirementV2(calculation.Value().RetirementJsonV2);
                var selectedQuoteDetails = _retirementService.GetSelectedQuoteDetails(request.SelectedQuoteName, retirementV2);
                selectedQuoteDetails.Add("totalLTAUsedPerc", retirementV2.GetTotalLtaUsedPerc(request.SelectedQuoteName, businessGroup, "DC"));
                selectedQuoteDetails.Add("selectedQuoteFullName", request.SelectedQuoteName);

                var newJourney = await _genericJourneyService.CreateJourney(
                        businessGroup,
                        referenceNumber,
                        journeyType,
                        request.CurrentPageKey,
                        request.NextPageKey,
                        false,
                        request.JourneyStatus);

                dcExploreOptionsData.IfSome(x => newJourney.SetWordingFlags(x.GetQuestionFormsWordingFlags()));
                var step = newJourney.GetStepByKey(request.CurrentPageKey).Value();
                step.UpdateGenericData("SelectedQuoteDetails", _jsonConversionService.Serialize(selectedQuoteDetails));

                step.AppendGenericDataList(dcExploreOptionsData.IsSome ? dcExploreOptionsData.Value().GetJourneyGenericDataList() : null);
                await _journeysRepository.Create(newJourney);
                if (dcExploreOptionsData.IsSome)
                    _journeysRepository.Remove(dcExploreOptionsData.Value());

                await _mdpUnitOfWork.Commit();
                return NoContent();
            });
    }

    [HttpPost("api/journeys/{journeyType}/start-db-core-journey")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> StartDbCoreJourney(string journeyType, [FromBody] StartDbCoreJourneyRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        return await _journeysRepository.Find(businessGroup, referenceNumber, journeyType)
            .ToAsync()
            .MatchAsync<IActionResult>(
            _ => 
            {
                _logger.LogWarning("A journey already exists for user with referenceNumber: {ReferenceNumber} in businessGroup: {BusinessGroup}", referenceNumber, businessGroup);
                return NoContent();
            },
            async () =>
            {
                var expiry = RetirementJourney.CalculateExpireDate(
                    request.RetirementDate,
                    DateTimeOffset.UtcNow,
                    RetirementConstants.RetirementProcessingPeriodInDays,
                    RetirementConstants.DbCoreRetirementJourneyExpiryInDays);

                var newJourney = new GenericJourney(
                  businessGroup,
                  referenceNumber,
                  journeyType,
                  request.CurrentPageKey,
                  request.NextPageKey,
                  request.RemoveOnLogin,
                  request.JourneyStatus,
                  DateTimeOffset.UtcNow,
                  expiry
                  );

                var member = await _memberRepository.FindMember(referenceNumber, businessGroup);
                if (member.IsNone)
                {
                    _logger.LogWarning("Member not found for referenceNumber: {ReferenceNumber} in businessGroup: {BusinessGroup}", referenceNumber, businessGroup);
                    return BadRequest(new ApiError("Requested member was not found")); 
                }
                var memberAgeAtSelectedRetirementDate = member.Value().AgeOnSelectedDate(request.RetirementDate);

                var selectedRetirementDateAndAgeDictionary = new Dictionary<string, object>
                {
                    { nameof(request.RetirementDate).ToFirstLower(), request.RetirementDate },
                    { "retirementAge", memberAgeAtSelectedRetirementDate }
                };

                var step = newJourney.GetStepByKey(request.CurrentPageKey);
                step.Value().UpdateGenericData("pre_journey_retirement_date_picker", JsonSerializer.Serialize(selectedRetirementDateAndAgeDictionary));

                _logger.LogInformation("Creating DB Core journey for user with referenceNumber: {ReferenceNumber} in businessGroup: {BusinessGroup}", referenceNumber, businessGroup );
                await _journeysRepository.Create(newJourney);
                await _mdpUnitOfWork.Commit();
                return NoContent();
            });
    }
}