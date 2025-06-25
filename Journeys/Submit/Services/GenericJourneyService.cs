using System;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Journeys.Submit.Services.Dto;
using WTW.MdpService.Retirement;
using WTW.Web.LanguageExt;
using WTW.Web.Serialization;

namespace WTW.MdpService.Journeys.Submit.Services;

public class GenericJourneyService : IGenericJourneyService
{
    private const string JourneySubmitionDetailsKey = "JourneySubmissionDetails";
    private readonly IJourneysRepository _journeysRepository;
    private readonly IMdpUnitOfWork _mdpUnitOfWork;
    private readonly IJsonConversionService _jsonConversionService;
    private readonly ICalculationsParser _calculationsParser;
    private readonly IRetirementService _retirementService;
    private readonly ILogger<GenericJourneyService> _logger;

    public GenericJourneyService(IJourneysRepository journeysRepository,
        IMdpUnitOfWork mdpUnitOfWork,
        IJsonConversionService jsonConversionService,
        ICalculationsParser calculationsParser,
        IRetirementService retirementService,
        ILogger<GenericJourneyService> logger)
    {
        _journeysRepository = journeysRepository;
        _mdpUnitOfWork = mdpUnitOfWork;
        _jsonConversionService = jsonConversionService;
        _calculationsParser = calculationsParser;
        _retirementService = retirementService;
        _logger = logger;
    }

    public async Task SetStatusSubmitted(string businessGroup, string referenceNumber, string journeyType)
    {
        _logger.LogInformation("Submitting journey. Journey type: {journeyType}.", journeyType);
        var journey = await _journeysRepository.Find(businessGroup, referenceNumber, journeyType);
        journey.Value().SubmitJourney(DateTimeOffset.UtcNow);
        await _mdpUnitOfWork.Commit();
    }

    public async Task SaveSubmissionDetailsToGenericData(string businessGroup, string referenceNumber, string journeyType, (string CaseNumber, int EdmsImageId) details)
    {
        _logger.LogInformation("Saving case number to generic data. Journey type: {journeyType}.", journeyType);
        var journey = await _journeysRepository.Find(businessGroup, referenceNumber, journeyType);
        journey.Value().GetFirstStep().UpdateGenericData(
            JourneySubmitionDetailsKey,
            _jsonConversionService.Serialize(new SubmissionDetailsDto { CaseNumber = details.CaseNumber, SummaryPdfEdmsImageId = details.EdmsImageId, SubmissionDate = journey.Value().SubmissionDate }));
        await _mdpUnitOfWork.Commit();
    }

    public async Task<Either<Error, SubmissionDetailsDto>> GetSubmissionDetailsFromGenericData(string businessGroup, string referenceNumber, string journeyType)
    {
        var journey = await _journeysRepository.Find(businessGroup, referenceNumber, journeyType);
        if (journey.IsNone)
            return Error.New($"Generic Journey with type \"{journeyType}\" is not found.");

        _logger.LogInformation("Getting case number from generic data. Journey type: {journeyType}.", journeyType);
        var genericData = journey.Value().GetFirstStep().GetGenericDataByKey(JourneySubmitionDetailsKey);
        if (genericData.IsNone)
            return Error.New($"Generic data with key {JourneySubmitionDetailsKey} not found in journey with type {journeyType}.");

        var dataJson = _jsonConversionService.Deserialize<SubmissionDetailsDto>(genericData.Value().GenericDataJson);
        return dataJson;
    }

    public async Task<GenericJourney> CreateJourney(string businessGroup, string referenceNumber, string journeyType, string currentPageKey, string nextPageKey, bool removeOnLogin, string journeyStatus)
    {
        var expiryDate = await CalculateJourneyExpiryDate(businessGroup, referenceNumber, journeyType);

        return new GenericJourney(
                  businessGroup,
                  referenceNumber,
                  journeyType,
                  currentPageKey,
                  nextPageKey,
                  removeOnLogin,
                  journeyStatus,
                  DateTimeOffset.UtcNow,
                  expiryDate);
    }

    private async Task<DateTimeOffset?> CalculateJourneyExpiryDate(string businessGroup, string referenceNumber, string journeyType)
    {
        return journeyType.ToLower() switch
        {
            "dbcoreretirementapplication" => DateTimeOffset.UtcNow.AddDays(RetirementConstants.DbCoreRetirementJourneyExpiryInDays),
            "dcretirementapplication" => await GetDcRetirementApplicationExpiryDate(businessGroup, referenceNumber),
            _ => null
        };
    }

    private async Task<DateTimeOffset> GetDcRetirementApplicationExpiryDate(string businessGroup, string referenceNumber)
    {
        return await _journeysRepository.Find(businessGroup, referenceNumber, "dcexploreoptions")
            .ToAsync()
            .Match(x =>
            {
                var now = DateTimeOffset.UtcNow;
                var genericData = x.GetStepByKey("DC_explore_options").Value().GetGenericDataByKey("DC_options_filter_retirement_date");
                if (genericData.IsNone)
                    return DateTimeOffset.UtcNow.AddDays(RetirementConstants.DcRetirementJourneyExpiryInDays);

                var data = _jsonConversionService.Deserialize<DcSubmissionRetirementDateDto>(genericData.Value().GenericDataJson);

                if (data.RetirementDate.AddDays(-RetirementConstants.DcRetirementProcessingPeriodInDays) < now.AddDays(RetirementConstants.DcRetirementJourneyExpiryInDays))
                    return data.RetirementDate.AddDays(-RetirementConstants.DcRetirementProcessingPeriodInDays);

                return now.AddDays(RetirementConstants.DcRetirementJourneyExpiryInDays);
            },
            () => DateTimeOffset.UtcNow.AddDays(RetirementConstants.DcRetirementJourneyExpiryInDays));
    }

    public async Task<GenericJourney> CreateJourney(string businessGroup, string referenceNumber, string journeyType, string currentPageKey, string nextPageKey, bool removeOnLogin, string journeyStatus, DateTimeOffset? expiryDate)
    {
        return new GenericJourney(
            businessGroup,
            referenceNumber,
            journeyType,
            currentPageKey,
            nextPageKey,
            removeOnLogin,
            journeyStatus,
            DateTimeOffset.UtcNow,
            expiryDate);
    }

    public async Task<bool> ExistsJourney(string businessGroup, string referenceNumber, string journeyType)
    {
        var journey = await _journeysRepository.Find(businessGroup, referenceNumber, journeyType);
        if (journey.IsNone)
        {
            _logger.LogInformation("For given user generic Journey with type \"{journeyType}\" is not found.", journeyType);
            return false;
        }

        return true;
    }

    public async Task UpdateDcRetirementSelectedJourneyQuoteDetails(Calculation calculation)
    {
        await _journeysRepository.FindUnexpired(calculation.BusinessGroup, calculation.ReferenceNumber, "dcretirementapplication")
            .ToAsync()
            .IfSomeAsync(async j =>
            {
                _logger.LogInformation("Start updating quote details. {bgroup}-{refno}", calculation.BusinessGroup, calculation.ReferenceNumber);
                var retirementV2 = _calculationsParser.GetRetirementV2(calculation.RetirementJsonV2);
                var step = j.GetStepByKey("DC_options_timetable").Value();
                var dto = _jsonConversionService.Deserialize<SelectedQuoteDetailsDto>(step.GetGenericDataByKey("SelectedQuoteDetails").Value().GenericDataJson);
                var selectedQuoteDetails = _retirementService.GetSelectedQuoteDetails(dto.SelectedQuoteFullName, retirementV2);
                selectedQuoteDetails.Add("totalLTAUsedPerc", retirementV2.GetTotalLtaUsedPerc(dto.SelectedQuoteFullName, calculation.BusinessGroup, "DC"));
                selectedQuoteDetails.Add("selectedQuoteFullName", dto.SelectedQuoteFullName);
                step.UpdateGenericData("SelectedQuoteDetails", _jsonConversionService.Serialize(selectedQuoteDetails));
                await _mdpUnitOfWork.Commit();
            });
    }
}