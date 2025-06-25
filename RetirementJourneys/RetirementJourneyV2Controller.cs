using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Retirement;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.RetirementJourneys;

[ApiController]
public class RetirementJourneyV2Controller : ControllerBase
{
    private readonly IRetirementJourneyRepository _retirementJourneyRepository;
    private readonly ICalculationsRepository _calculationsRepository;
    private readonly IMdpUnitOfWork _mdpDbUnitOfWork;
    private readonly RetirementJourneyConfiguration _retirementJourneyConfiguration;
    private readonly IQuoteSelectionJourneyRepository _quoteSelectionJourneyRepository;
    private readonly ICalculationsClient _calculationsClient;
    private readonly ICalculationsParser _calculationsParser;
    private readonly ILogger<RetirementJourneyV2Controller> _logger;

    public RetirementJourneyV2Controller(IRetirementJourneyRepository retirementJourneyRepository,
        ICalculationsRepository calculationsRepository,
        IMdpUnitOfWork mdpDbUnitOfWork,
        RetirementJourneyConfiguration retirementJourneyConfiguration,
        IQuoteSelectionJourneyRepository quoteSelectionJourneyRepository,
        ICalculationsClient calculationsClient,
        ICalculationsParser calculationsParser,
        ILogger<RetirementJourneyV2Controller> logger)
    {
        _retirementJourneyRepository = retirementJourneyRepository;
        _calculationsRepository = calculationsRepository;
        _mdpDbUnitOfWork = mdpDbUnitOfWork;
        _retirementJourneyConfiguration = retirementJourneyConfiguration;
        _quoteSelectionJourneyRepository = quoteSelectionJourneyRepository;
        _calculationsClient = calculationsClient;
        _calculationsParser = calculationsParser;
        _logger = logger;
    }

    [HttpPost("api/v3/retirement-journey/start")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> StartJourneyV2([FromBody] StartRetirementJourneyV3Request request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var result = await _retirementJourneyRepository.FindExpiredJourney(businessGroup, referenceNumber);
        if (result.IsSome)
            _retirementJourneyRepository.Remove(result.Single());

        var calculation = await _calculationsRepository.Find(referenceNumber, businessGroup);
        if (calculation.IsNone)
            return BadRequest(ApiError.FromMessage("Calculation does not exist."));

        if (calculation.Value().IsRetirementDateOutOfRange())
            return BadRequest(ApiError.FromMessage("Effective retirement date is out of range."));

        var retirementDatesAges = _calculationsParser.GetRetirementDatesAges(calculation.Value().RetirementDatesAgesJson);
        var retirementV2 = _calculationsParser.GetRetirementV2(calculation.Value().RetirementJsonV2);
        var quoteSelectionJourney = await _quoteSelectionJourneyRepository.Find(businessGroup, referenceNumber);
        if (quoteSelectionJourney.IsNone)
            return BadRequest(ApiError.FromMessage("Quote Selection journey does not exist."));

        var selectedQuoteName = quoteSelectionJourney.Value().QuoteSelection();
        if (selectedQuoteName.IsNone)
            return BadRequest(ApiError.FromMessage("Selected quote name does not exist."));

        var quote = MemberQuote.CreateV2(
            calculation.Value().EffectiveRetirementDate.ToUniversalTime(),
            selectedQuoteName.Value(),
            retirementV2.HasAdditionalContributions(),
            retirementV2.TotalLtaUsedPercentage,
            decimal.ToInt32(retirementDatesAges.EarliestRetirementAge),
            decimal.ToInt32(retirementDatesAges.NormalRetirementAge),
            retirementDatesAges.NormalRetirementDate.ToUniversalTime(),
            retirementV2.DatePensionableServiceCommenced?.ToUniversalTime(),
            retirementV2.DateOfLeaving?.ToUniversalTime(),
            retirementV2.TransferInService,
            retirementV2.TotalPensionableService,
            retirementV2.FinalPensionableSalary,
            retirementV2.CalculationEventType,
            retirementV2.WordingFlagsAsString(),
            retirementV2.StatePensionDeduction);

        if (quote.IsLeft)
            return BadRequest(ApiError.FromMessage(quote.Left().Message));

        var journey = new RetirementJourney(
            businessGroup,
            referenceNumber,
            DateTimeOffset.UtcNow,
            request.CurrentPageKey,
            request.NextPageKey,
            quote.Right(),
            _retirementJourneyConfiguration.RetirementJourneyDaysToExpire,
            RetirementConstants.RetirementProcessingPeriodInDays);

        await using var transaction = await _mdpDbUnitOfWork.BeginTransactionAsync();
        try
        {
            await _retirementJourneyRepository.Create(journey);
            await _mdpDbUnitOfWork.Commit();
            _logger.LogInformation("Member: {bgroup}-{refno}.On starting DB journey calc api retirement data: {retirementJson}. Selected quote name: {quoteName}.",
             businessGroup, referenceNumber, calculation.Value().RetirementJsonV2, selectedQuoteName.Value());
            var retirementOrError = await _calculationsClient.RetirementCalculationV2WithLock(referenceNumber,
                businessGroup,
                retirementV2.CalculationEventType,
                calculation.Value().EffectiveRetirementDate,
                calculation.Value().EnteredLumpSum);
            if (retirementOrError.IsLeft)
                throw new Exception(retirementOrError.Left().Message);

            var (retirementJsonV2, mdp) = _calculationsParser.GetRetirementJsonV2(retirementOrError.Right(), retirementV2.CalculationEventType);
            _logger.LogInformation("Member: {bgroup}-{refno}.After starting DB journey calc api retirement data: {retirementJson}. Selected quote name: {quoteName}.",
               businessGroup, referenceNumber, retirementJsonV2, selectedQuoteName.Value());
            calculation.Value().UpdateRetirementV2(retirementJsonV2, mdp, calculation.Value().EffectiveRetirementDate, DateTimeOffset.UtcNow);
            calculation.Value().SetJourney(journey, selectedQuoteName.Value());
            await _mdpDbUnitOfWork.Commit();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start retirement journey. Exception: ");
            await transaction.RollbackAsync();
            throw;
        }

        return NoContent();
    }
}