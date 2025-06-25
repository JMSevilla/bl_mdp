using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.UnsafeValueAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure;
using WTW.MdpService.Infrastructure.Aws;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.Investment;
using WTW.MdpService.Infrastructure.Journeys;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.MdpService.Infrastructure.Redis;
using WTW.MdpService.Infrastructure.Templates.RetirementApplication.Calculations;
using WTW.MdpService.Journeys.Submit.Services;
using WTW.MdpService.Members;
using WTW.MdpService.TransferJourneys;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;
using WTW.Web.Models.Internal;
using static WTW.MdpService.Retirement.GuaranteedQuotesRequest;

namespace WTW.MdpService.Retirement;

[ApiController]
public class RetirementController : ControllerBase
{
    private readonly ICalculationsClient _calculationsClient;
    private readonly IMemberRepository _memberRepository;
    private readonly ICalculationsRepository _calculationsRepository;
    private readonly ITransferCalculationRepository _transferCalculationRepository;
    private readonly ITenantRetirementTimelineRepository _tenantRetirementTimelineRepository;
    private readonly IBankHolidayRepository _bankHolidayRepository;
    private readonly ITransferJourneyRepository _transferJourneyRepository;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<RetirementController> _logger;
    private readonly IMdpUnitOfWork _mdpUnitOfWork;
    private readonly IRetirementDatesService _retirementDatesService;
    private readonly IRetirementCalculationsPdf _retirementCalculationsPdf;
    private readonly ICalculationsParser _calculationsParser;
    private readonly IGenericJourneyService _genericJourneyService;
    private readonly IAwsClient _awsClient;
    private readonly IRateOfReturnService _rateOfReturnService;
    private readonly IDocumentsUploaderService _documentsUploaderService;
    private readonly IJourneysRepository _journeysRepository;
    private readonly DatePickerConfigOptions _datePickerConfigOptions;
    private readonly IJourneyService _journeyService;

    public RetirementController(
        ICalculationsClient calculationsClient,
        IMemberRepository memberRepository,
        ICalculationsRepository calculationsRepository,
        ITransferCalculationRepository transferCalculationRepository,
        IMdpUnitOfWork unitOfWork,
        ITenantRetirementTimelineRepository tenantRetirementTimelineRepository,
        IBankHolidayRepository bankHolidayRepository,
        ICalculationsRedisCache calculationsRedisCache,
        ITransferJourneyRepository transferJourneyRepository,
        ILoggerFactory loggerFactory,
        ILogger<RetirementController> logger,
        IJourneyDocumentsRepository journeyDocumentsRepository,
        IRetirementCalculationsPdf retirementCalculationsPdf,
        ITransferOutsideAssure transferOutsideAssure,
        IRetirementDatesService retirementDatesService,
        ICalculationsParser calculationsParser,
        IGenericJourneyService genericJourneyService,
        IAwsClient awsClient,
        IOptionsSnapshot<DatePickerConfigOptions> datePickerConfigOptions,
        IRateOfReturnService rateOfReturnService,
        IDocumentsUploaderService documentsUploaderService,
        IJourneysRepository journeysRepository,
        IJourneyService journeyService
        )
    {
        _calculationsClient = calculationsClient;
        _memberRepository = memberRepository;
        _calculationsRepository = calculationsRepository;
        _transferCalculationRepository = transferCalculationRepository;
        _mdpUnitOfWork = unitOfWork;
        _bankHolidayRepository = bankHolidayRepository;
        _transferJourneyRepository = transferJourneyRepository;
        _tenantRetirementTimelineRepository = tenantRetirementTimelineRepository;
        _loggerFactory = loggerFactory;
        _logger = logger;
        _retirementCalculationsPdf = retirementCalculationsPdf;
        _calculationsParser = calculationsParser;
        _retirementDatesService = retirementDatesService;
        _genericJourneyService = genericJourneyService;
        _awsClient = awsClient;
        _rateOfReturnService = rateOfReturnService;
        _documentsUploaderService = documentsUploaderService;
        _journeysRepository = journeysRepository;
        _datePickerConfigOptions = datePickerConfigOptions.Value;
        _journeyService = journeyService;
    }

    [HttpGet]
    [Route("api/retirement/calculation")]
    [ProducesResponseType(typeof(RetirementCalculationResponse), 200)]
    public async Task<IActionResult> RetirementCalculation()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        var member = (await _memberRepository.FindMember(referenceNumber, businessGroup)).Value();
        if (!member.CanCalculateRetirement())
            return Ok(RetirementCalculationResponse.CalculationFailed());

        var calculation = await _calculationsRepository.Find(referenceNumber, businessGroup);

        if (calculation.IsSome && calculation.Value().IsCalculationSuccessful == false)
            return Ok(RetirementCalculationResponse.CalculationFailed());

        var retirementDatesAgesResponse = await _calculationsClient.RetirementDatesAges(referenceNumber, businessGroup).Try();
        if (!retirementDatesAgesResponse.IsSuccess)
        {
            _logger.LogError("RetirementDatesAges - Unable to derive Effective Date.");
            return Ok(RetirementCalculationResponse.CalculationFailed());
        }

        var retirementDateAges = new RetirementDatesAges(retirementDatesAgesResponse.Value());
        var effectiveDate = retirementDateAges.EffectiveDate(DateTimeOffset.UtcNow, businessGroup).Date;

        if (_calculationsParser.IsGuaranteedQuoteEnabled(businessGroup))
        {
            var getGuaranteedQuoteClientRequest = new GetGuaranteedQuoteClientRequest
            {
                Bgroup = businessGroup,
                RefNo = referenceNumber,
                QuotationStatus = GuaranteedQuotesRequest.QuotationStatusValue.GUARANTEED.ToString(),
            };

            var guaranteedQuotes = await _calculationsClient.GetGuaranteedQuotes(getGuaranteedQuoteClientRequest);
            if (guaranteedQuotes.IsLeft)
            {
                _logger.LogError("GetGuaranteedQuotes failed with error: {ErrorMessage}", guaranteedQuotes.Left().Message);
                return Ok(RetirementCalculationResponse.CalculationFailed());
            }
            if (guaranteedQuotes.Right().Quotations.Any())
            {
                effectiveDate = guaranteedQuotes.Right().Quotations.OrderByDescending(x => x.RunDate).First().EffectiveDate.Value;
            }
        }

        return (await _calculationsClient.RetirementCalculationV2(referenceNumber, businessGroup, effectiveDate, false, false))
            .Match<IActionResult>(
                c => Ok(RetirementCalculationResponse.From(new RetirementV2Mapper().MapToDomain(c.RetirementResponseV2, c.EventType))),
                error => Ok(RetirementCalculationResponse.CalculationFailed())
            );
    }


    [HttpGet]
    [Route("api/retirement/options")]
    [ProducesResponseType(typeof(OptionsResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GetOptions([Required, Range(1, 150)] int age)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var member = (await _memberRepository.FindMember(referenceNumber, businessGroup)).Value();

        if (!member.CanCalculateRetirement())
            return Ok(new OptionsResponse { IsCalculationSuccessful = false });

        return (await _calculationsClient.RetirementCalculationV2(referenceNumber, businessGroup, member.PersonalDetails.DateOfBirth.Value.AddYears(age).Date, false))
           .Match<IActionResult>(
               c =>
               {
                   var retirement = new RetirementV2Mapper().MapToDomain(c.RetirementResponseV2, c.EventType);

                   return Ok(OptionsResponse.From(retirement.FullPensionYearlyIncome(), retirement.MaxLumpSum(), retirement.MaxLumpSumYearlyIncome()));
               },
               error => Ok(OptionsResponse.CalculationFailed())
           );
    }

    [HttpGet]
    [Route("api/v3/retirement/quotes")]
    [ProducesResponseType(typeof(QuotesResponseV2), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> QuotesV3([FromQuery] RetirementQuotesRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var now = DateTimeOffset.UtcNow;
        var member = (await _memberRepository.FindMember(referenceNumber, businessGroup));
        if (member.IsNone)
            return NotFound(ApiError.NotFound());

        if (!member.Value().HasDateOfBirth() || !member.Value().CanCalculateRetirement())
            return Ok(QuotesResponseV2.CalculationFailed());

        if (await _calculationsRepository.FindWithValidRetirementJourney(referenceNumber, businessGroup, now) is var calculationWithJourney
            && calculationWithJourney.IsSome)
        {
            var retirementV2 = _calculationsParser.GetRetirementV2(calculationWithJourney.Value().RetirementJsonV2);
            var quotesV2 = _calculationsParser.GetQuotesV2(calculationWithJourney.Value().QuotesJsonV2);
            return Ok(QuotesResponseV2.From(quotesV2, retirementV2.WordingFlags, retirementV2.TotalAVCFund()));
        }

        return await _calculationsRepository.Find(referenceNumber, businessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async calculation =>
                {
                    if (calculation.IsCalculationSuccessful == false)
                        return Ok(QuotesResponseV2.CalculationFailed());

                    var retirementDate = calculation.RetirementDateWithAppliedRetirementProcessingPeriod(RetirementConstants.RetirementProcessingPeriodInDays, member.Value().Scheme?.Type, now);
                    var selectedRetirementDate = request.SelectedRetirementDate ?? retirementDate;
                    if (calculation.EnteredLumpSum.HasValue && calculation.EffectiveRetirementDate == selectedRetirementDate.ToUniversalTime())
                    {
                        var retirementFromDb = _calculationsParser.GetRetirementV2(calculation.RetirementJsonV2);
                        var quotesV2 = _calculationsParser.GetQuotesV2(calculation.QuotesJsonV2);
                        return Ok(QuotesResponseV2.From(quotesV2, retirementFromDb.WordingFlags, retirementFromDb.TotalAVCFund()));
                    }

                    bool engineGuaranteeQuote = (!member.Value().Status.Equals(MemberStatus.Active)) ? IsWithinThreeMonths(selectedRetirementDate) : false;

                    var retirementOrError = await _calculationsClient.RetirementCalculationV2(referenceNumber, businessGroup, selectedRetirementDate, engineGuaranteeQuote, request.BypassCache);

                    if (retirementOrError.IsLeft)
                    {
                        calculation.UpdateCalculationSuccessStatus(false);
                        await _mdpUnitOfWork.Commit();
                        return Ok(QuotesResponseV2.CalculationFailed());
                    }

                    var (retirementV2, mdp) = _calculationsParser.GetRetirementJsonV2(retirementOrError.Right().RetirementResponseV2, retirementOrError.Right().EventType);

                    var (quoteGuaranteed, quoteExpiryDate) = _calculationsParser.GetGuaranteedQuoteDetail(retirementOrError.Right().RetirementResponseV2);

                    calculation.UpdateRetirementV2(
                        retirementV2,
                        mdp,
                        selectedRetirementDate.ToUniversalTime(),
                        now,
                        quoteGuaranteed,
                        quoteExpiryDate
                        );
                    calculation.UpdateCalculationSuccessStatus(true);

                    await _mdpUnitOfWork.Commit();
                    var retirement = _calculationsParser.GetRetirementV2(calculation.RetirementJsonV2);
                    return Ok(QuotesResponseV2.From(retirementOrError.Right().RetirementResponseV2.Results.Mdp, retirement.WordingFlags, retirement.TotalAVCFund()));
                },
                async () =>
                {
                    var newCalculation = await NewCalculationV2(member.Value(), referenceNumber, businessGroup, now, request.BypassCache, request.SelectedRetirementDate?.ToUniversalTime());
                    if (newCalculation.IsNone)
                        return Ok(QuotesResponseV2.CalculationFailed());

                    await _calculationsRepository.Create(newCalculation.Value());
                    await _mdpUnitOfWork.Commit();

                    var retirement = _calculationsParser.GetRetirementV2(newCalculation.Value().RetirementJsonV2);
                    var quotesV2 = _calculationsParser.GetQuotesV2(newCalculation.Value().QuotesJsonV2);
                    return Ok(QuotesResponseV2.From(quotesV2, retirement.WordingFlags, retirement.TotalAVCFund()));
                });
    }
    static bool IsWithinThreeMonths(DateTime date)
    {
        DateTime today = DateTime.Today;
        DateTime threeMonthsFromNow = today.AddMonths(3);
        return date >= today && date <= threeMonthsFromNow;
    }

    [HttpGet]
    [Route("api/v3/retirement/guaranteed-quotes")]
    [ProducesResponseType(typeof(GetGuaranteedQuoteResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GuaranteedQuotes([FromQuery] GuaranteedQuotesRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        return await _memberRepository.FindMember(referenceNumber, businessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
            async member =>
            {
                var getGuaranteedQuoteClientRequest = new GetGuaranteedQuoteClientRequest
                {
                    Bgroup = businessGroup,
                    RefNo = referenceNumber,
                    Event = request.Event,
                    GuaranteeDateFrom = request.GuaranteeDateFrom,
                    GuaranteeDateTo = request.GuaranteeDateTo,
                    PageNumber = request.PageNumber.Value,
                    PageSize = request.PageSize.Value,
                    QuotationStatus = request.QuotationStatus,
                };

                var guaranteedQuotes = await _calculationsClient.GetGuaranteedQuotes(getGuaranteedQuoteClientRequest);

                if (guaranteedQuotes.IsLeft)
                {
                    _logger.LogError("GetGuaranteedQuotes failed with error: {message}", guaranteedQuotes.Left().Message);

                    return Ok(guaranteedQuotes.Left());
                }

                return Ok(guaranteedQuotes.Right());
            },
            () => NotFound(ApiError.NotFound()));
    }

    private async Task<Option<Domain.Mdp.Calculations.Calculation>> NewCalculationV2(Member member, string referenceNumber, string businessGroup, DateTimeOffset utcNow, bool bypassCache, DateTime? selectedRetirementDate)
    {
        var retirementDatesAgesResponse = await _calculationsClient.RetirementDatesAges(referenceNumber, businessGroup).Try();
        if (!retirementDatesAgesResponse.IsSuccess)
            return Option<Domain.Mdp.Calculations.Calculation>.None;

        var retirementDatesAges = new RetirementDatesAges(retirementDatesAgesResponse.Value());
        var retirementDatesAgesJson = _calculationsParser.GetRetirementDatesAgesJson(retirementDatesAgesResponse.Value());
        var effectiveDate = retirementDatesAges.EffectiveDate(utcNow, businessGroup).Date;
        if (member.IsSchemeDc())
            effectiveDate = member.DcRetirementDate(utcNow);

        var effectiveRetirementDate = selectedRetirementDate ?? effectiveDate;
        var retirementOrError = await _calculationsClient.RetirementCalculationV2(referenceNumber, businessGroup, effectiveRetirementDate, bypassCache);
        if (retirementOrError.IsLeft)
            return Option<Domain.Mdp.Calculations.Calculation>.None;

        var (retirementJsonV2, mdp) = _calculationsParser.GetRetirementJsonV2(retirementOrError.Right().RetirementResponseV2, retirementOrError.Right().EventType);

        return new Domain.Mdp.Calculations.Calculation(referenceNumber, businessGroup, retirementDatesAgesJson, retirementJsonV2, mdp, effectiveRetirementDate, utcNow, true);
    }

    [HttpGet]
    [Route("api/v3/retirement/quotes/recalculate-lumpsum")]
    [ProducesResponseType(typeof(RecalculatedLumpSumResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> RecalculateLumpSum([Required, Range(1, double.MaxValue)] decimal requestedLumpSum)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        var member = (await _memberRepository.FindMember(referenceNumber, businessGroup));
        if (member.IsNone)
            return NotFound(ApiError.NotFound());

        if (!member.Value().HasDateOfBirth() || !member.Value().CanCalculateRetirement())
            return Ok(new RecalculatedLumpSumResponse().CalculationFailed());

        return await _calculationsRepository.Find(referenceNumber, businessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async calculation =>
                {
                    var factorDate = _calculationsParser.GetCalculationFactorDate(calculation.QuotesJsonV2);

                    var retirementOrError = await _calculationsClient.RetirementCalculationV2(referenceNumber, businessGroup, calculation.EffectiveRetirementDate, requestedLumpSum, factorDate);
                    if (retirementOrError.IsLeft)
                        return Ok(new RecalculatedLumpSumResponse().CalculationFailed());

                    return Ok(new RecalculatedLumpSumResponse(retirementOrError.Right().RetirementResponseV2.Results.Mdp));
                },
                () => Ok(new RecalculatedLumpSumResponse().CalculationFailed()));
    }

    [HttpPut]
    [Route("api/v3/retirement/quotes/clear-lumpsum")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> ClearLumpSum()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var member = (await _memberRepository.FindMember(referenceNumber, businessGroup));
        if (member.IsNone)
            return NotFound(ApiError.NotFound());

        return await _calculationsRepository.Find(referenceNumber, businessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async calculation =>
                {
                    calculation.ClearLumpSum();
                    await _mdpUnitOfWork.Commit();
                    return NoContent();
                },
                () => BadRequest(ApiError.FromMessage("Calculation does not exist.")));
    }

    [HttpPost]
    [Route("api/v3/retirement/quotes/submit-recalculated-lumpsum")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(QuotesResponse), 200)]
    public async Task<IActionResult> SubmitRecalculateLumpSum([Required, Range(1, double.MaxValue)] decimal requestedLumpSum)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var member = (await _memberRepository.FindMember(referenceNumber, businessGroup));
        if (member.IsNone)
            return BadRequest(ApiError.FromMessage("Member does not exist."));

        return await _calculationsRepository.Find(referenceNumber, businessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async calculation =>
                {
                    var retirementOrError = await _calculationsClient.RetirementCalculationV2(referenceNumber, businessGroup, calculation.EffectiveRetirementDate, requestedLumpSum);
                    if (retirementOrError.IsLeft)
                        return Ok(QuotesResponse.CalculationFailed());

                    var (retirementV2, mdp) = _calculationsParser.GetRetirementJsonV2(retirementOrError.Right().RetirementResponseV2, retirementOrError.Right().EventType);

                    calculation.UpdateRetirementV2(
                        retirementV2,
                        mdp,
                        calculation.EffectiveRetirementDate,
                        DateTimeOffset.UtcNow);
                    calculation.SetEnteredLumpSum(requestedLumpSum);
                    await _mdpUnitOfWork.Commit();
                    await _genericJourneyService.UpdateDcRetirementSelectedJourneyQuoteDetails(calculation);
                    return NoContent();
                },
                () => BadRequest(ApiError.FromMessage("Calculation does not exist.")));
    }

    [HttpGet]
    [Route("api/v3/retirement/quotes/download/summary-pdf")]
    [ProducesResponseType(typeof(FileResult), 200, "application/octet-stream")]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> GenerateSummaryPdf([FromQuery][Required] string contentAccessKey, [Required] string summaryKey)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var member = (await _memberRepository.FindMember(referenceNumber, businessGroup));
        if (member.IsNone)
            return NotFound(ApiError.NotFound());

        return await _calculationsRepository.Find(referenceNumber, businessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async calculation =>
                {
                    var summaryPdf = await _retirementCalculationsPdf.GenerateSummaryPdf(
                        contentAccessKey,
                        calculation,
                        member.Value(),
                        summaryKey,
                        businessGroup,
                        Request.Headers[HeaderNames.Authorization],
                        Request.Headers["env"]);

                    return File(summaryPdf.ToArray(), "application/pdf", "retirement-quotes-summary.pdf");
                },
                () => BadRequest(ApiError.FromMessage("Calculation does not exist.")));
    }

    [HttpGet]
    [Route("api/v3/retirement/quotes/download/options-pdf")]
    [ProducesResponseType(typeof(FileResult), 200, "application/octet-stream")]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> GenerateOptionsPdf([FromQuery][Required] string contentAccessKey)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var member = (await _memberRepository.FindMember(referenceNumber, businessGroup));
        if (member.IsNone)
        {
            _logger.LogError("Member not found for reference number {ReferenceNumber} and business group {BusinessGroup}", referenceNumber, businessGroup);
            return NotFound(ApiError.NotFound());
        }

        return await _calculationsRepository.Find(referenceNumber, businessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async calculation =>
                {
                    var optionsPdf = await _retirementCalculationsPdf.GenerateOptionsPdf(
                        contentAccessKey,
                        calculation,
                        member.Value(),
                        businessGroup,
                        Request.Headers[HeaderNames.Authorization],
                        Request.Headers["env"]);
                    return File(optionsPdf.ToArray(), "application/pdf", "retirement-options.pdf");
                },
                () =>
                {
                    _logger.LogError("Calculation does not exist");
                    return BadRequest(ApiError.FromMessage("Calculation does not exist."));
                });
    }

    [HttpGet]
    [Route("api/v2/retirement/retirement-date")]
    [ProducesResponseType(typeof(RetirementDateResponse), 200)]
    public async Task<IActionResult> GetRetirementDatev2()
    {
        var now = DateTimeOffset.UtcNow;
        bool GMPOnlyMember = false;
        var memberJuridiction = string.Empty;

        (string userId, string referenceNumber, string businessGroup) = HttpContext.User.User();

        var member = (await _memberRepository.FindMember(referenceNumber, businessGroup));

        if (member.IsNone)
            return NotFound(ApiError.NotFound());

        if (!member.Value().CanCalculateRetirement())
            return Ok(RetirementDateResponse.CalculationFailed());

        List<DateTime> effectiveDateList = new List<DateTime>();

        if (_calculationsParser.IsGuaranteedQuoteEnabled(businessGroup))
        {
            var getGuaranteedQuoteClientRequest = new GetGuaranteedQuoteClientRequest
            {
                Bgroup = businessGroup,
                RefNo = referenceNumber,
                QuotationStatus = GuaranteedQuotesRequest.QuotationStatusValue.GUARANTEED.ToString(),
            };

            var guaranteedQuotes = await _calculationsClient.GetGuaranteedQuotes(getGuaranteedQuoteClientRequest);
            if (guaranteedQuotes.IsLeft)
            {
                _logger.LogError("GetGuaranteedQuotes failed with error: {ErrorMessage}", guaranteedQuotes.Left().Message);
                return Ok(RetirementCalculationResponse.CalculationFailed());
            }

            effectiveDateList = guaranteedQuotes.Right().Quotations.Any() ? guaranteedQuotes.Right().Quotations.Where(x => x.EffectiveDate < now).Select(x => x.EffectiveDate.Value).ToList() : null;

            GMPOnlyMember = await _calculationsParser.IsMemberGMPONly(member.Value(), userId);
            memberJuridiction = await _calculationsParser.GetMemberJuridiction(member.Value(), userId);
        }

        return (await _calculationsRepository.Find(referenceNumber, businessGroup))
            .Match<IActionResult>(
                 calculation =>
                {
                    var retirementDate = calculation.RetirementDateWithAppliedRetirementProcessingPeriod(RetirementConstants.RetirementProcessingPeriodInDays, member.Value().Scheme?.Type, now);
                    var dateOfBirth = member.Value().PersonalDetails.DateOfBirth.Value;

                    var retirementDatesAges = _calculationsParser.GetRetirementDatesAges(calculation.RetirementDatesAgesJson);

                    if (_datePickerConfigOptions.BusinessGroups.SingleOrDefault(x => x.Bgroup == businessGroup) is var groupe && groupe != null)
                    {
                        var availableRetirementDateToUsingConfig = retirementDatesAges.LastAvailableQuoteDate(now, groupe);
                        var availableRetirementDateFromUsingConfig = retirementDatesAges.FirstAvailableQuoteDate(now, groupe);
                        return Ok(RetirementDateResponse.From(retirementDate, dateOfBirth, calculation.QuoteExpiryDate, calculation.GuaranteedQuote, availableRetirementDateFromUsingConfig, availableRetirementDateToUsingConfig, effectiveDateList,
                            (calculation.IsCalculationSuccessful.HasValue && calculation.IsCalculationSuccessful.Value)));
                    }

                    if (calculation.IsCalculationSuccessful == false)
                    {
                        _logger.LogInformation("Calculations from calc api are failed state.");
                        return Ok(RetirementDateResponse.CalculationFailed());
                    }
                    var availableRetirementDateFrom = retirementDatesAges.EarliestRetirementDateWithAppliedRetirementProcessingPeriod(
                    RetirementConstants.RetirementProcessingPeriodInDays, now);

                    var availableRetirementDateTo = member.Value().LatestRetirementDate(retirementDatesAges.LatestRetirementDate, retirementDatesAges.GetLatestRetirementAge(), businessGroup, now);

                    availableRetirementDateFrom = _calculationsParser.IsGuaranteedQuoteEnabled(businessGroup) ? DateTime.UtcNow.AddDays(1) : availableRetirementDateFrom;

                    if (_calculationsParser.IsGuaranteedQuoteEnabled(businessGroup) && (GMPOnlyMember || !string.IsNullOrEmpty(memberJuridiction)))
                    {
                        (availableRetirementDateFrom, availableRetirementDateTo) = _calculationsParser.EvaluateDateRangeForGMPOrCrownDependencyMember(member.Value(), GMPOnlyMember, memberJuridiction, availableRetirementDateFrom, availableRetirementDateTo);
                    }

                    return Ok(RetirementDateResponse.From(retirementDate, dateOfBirth, calculation.QuoteExpiryDate, calculation.GuaranteedQuote, availableRetirementDateFrom, availableRetirementDateTo, effectiveDateList));
                },
                () => Ok(RetirementDateResponse.CalculationFailed()));
    }

    [HttpGet]
    [Route("api/retirement/transfer-quote")]
    [ProducesResponseType(typeof(TransferOptionsResponse), 200)]
    public async Task<IActionResult> GetTransferQuote()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        try
        {
            var member = (await _memberRepository.FindMember(referenceNumber, businessGroup)).Value();
            var now = DateTimeOffset.UtcNow;

            var transferCalculation = await _transferCalculationRepository.Find(businessGroup, referenceNumber);

            var datesAges = await _calculationsClient.RetirementDatesAges(referenceNumber, businessGroup).Try();
            var transferJourney = await _transferJourneyRepository.Find(businessGroup, referenceNumber);
            if (datesAges.IsSuccess && !datesAges.Value().HasLockedInTransferQuote && member.TransferPaperCase().IsNone)
            {
                transferJourney.IfSome(x => _transferJourneyRepository.Remove(x));
                transferCalculation.IfSome(x => _transferCalculationRepository.Remove(x));
                transferCalculation = Option<TransferCalculation>.None;
                await _mdpUnitOfWork.Commit();
            }

            if (datesAges.IsFaulted && transferCalculation.IsNone)
            {
                await _transferCalculationRepository.CreateIfNotExists(new TransferCalculation(businessGroup, referenceNumber, null, now));
                await _mdpUnitOfWork.Commit();
                return Ok(TransferOptionsResponse.CalculationFailed());
            }

            return await transferCalculation
                .ToAsync()
                .MatchAsync<IActionResult>(async calc =>
                {
                    if (calc.TransferQuoteJson == null)
                        return Ok(TransferOptionsResponse.CalculationFailed());

                    return Ok(new TransferOptionsResponse(
                        _calculationsParser.GetTransferQuote(calc.TransferQuoteJson),
                         member.GetTransferApplicationStatus(calc)));

                }, async () =>
                {
                    _logger.LogInformation("Checking if member {businessGroup}:{referenceNumber} is valid for transfer calculation.", businessGroup, referenceNumber);
                    var isMemberValidForTransferCalculation = await _memberRepository.IsMemberValidForTransferCalculation(referenceNumber, businessGroup);
                    if (!isMemberValidForTransferCalculation)
                    {
                        _logger.LogWarning("Transfer calculation is forbidden for member {businessGroup}:{referenceNumber}", businessGroup, referenceNumber);
                        await _transferCalculationRepository.CreateIfNotExists(new TransferCalculation(businessGroup, referenceNumber, null, now));
                        await _mdpUnitOfWork.Commit();
                        return Ok(TransferOptionsResponse.CalculationFailed());
                    }

                    _logger.LogInformation("Member {businessGroup}:{referenceNumber} is valid for transfer calculation. Running transfer calculation", businessGroup, referenceNumber);
                    var transferResponseOrError = await _calculationsClient.TransferCalculation(businessGroup, referenceNumber);
                    if (transferResponseOrError.IsLeft)
                    {
                        _logger.LogWarning("Transfer calculation failed for member {businessGroup}:{referenceNumber}. Error: {error}", businessGroup, referenceNumber, transferResponseOrError.Left().Message);
                        var calculation = new TransferCalculation(businessGroup, referenceNumber, null, now);
                        await _transferCalculationRepository.CreateIfNotExists(calculation);
                        await _mdpUnitOfWork.Commit();
                        return Ok(TransferOptionsResponse.CalculationFailed());
                    }

                    var transferQuoteJson = _calculationsParser.GetTransferQuoteJson(transferResponseOrError.Right());
                    var transferQuote = _calculationsParser.GetTransferQuote(transferQuoteJson);
                    var newTransferCalculation = new TransferCalculation(businessGroup, referenceNumber, transferQuoteJson, now);
                    await _transferCalculationRepository.CreateIfNotExists(newTransferCalculation);

                    if (transferJourney.IsNone && datesAges.Value().HasLockedInTransferQuote)
                    {
                        var journey = TransferJourney.Create(businessGroup, referenceNumber, DateTimeOffset.UtcNow, datesAges.Value().LockedInTransferQuoteImageId ?? 0); //todo: hard coded image id value for now.
                        await _transferJourneyRepository.Create(journey);
                        newTransferCalculation.LockTransferQoute();
                    }

                    await _mdpUnitOfWork.Commit();

                    return Ok(new TransferOptionsResponse(
                        transferQuote,
                        member.GetTransferApplicationStatus(newTransferCalculation)));
                });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "DbUpdateConcurrencyException occurred. Refno: {refNo}, BGroup: {bgroup}", referenceNumber, businessGroup);
            return Ok(TransferOptionsResponse.CalculationFailed());
        }
    }

    [HttpGet]
    [Route("api/retirement/cms-token-information")]
    [ProducesResponseType(typeof(CmsTokenInformationResponse), 200)]
    public async Task<IActionResult> GetCmsInformationForToken()
    {
        var now = DateTimeOffset.UtcNow;
        (string userId, string referenceNumber, string businessGroup) = HttpContext.User.User();

        var member = (await _memberRepository.FindMember(referenceNumber, businessGroup)).Value();

        if (!member.CanCalculateRetirement())
            return Ok(new CmsTokenInformationResponseBuilder()
                        .WithMemberData(member, null, now).Build());

        DateTimeOffset? submissionDate = null;

        var retirementJourney = await _journeyService.FindUnexpiredOrSubmittedJourney(businessGroup, referenceNumber);

        if (retirementJourney.IsSome)
            submissionDate = retirementJourney.Value().SubmissionDate;


        return await _calculationsRepository.Find(referenceNumber, businessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async calc =>
                {
                    var retirementDate = calc.RetirementDateWithAppliedRetirementProcessingPeriod(RetirementConstants.RetirementProcessingPeriodInDays, member.Scheme?.Type, now);
                    var retirementDatesAges = _calculationsParser.GetRetirementDatesAges(calc.RetirementDatesAgesJson);
                    var retirementExpirationDate = await _retirementDatesService.GetRetirementApplicationExpiryDate(calc, member, now);
                    var timelines = await _tenantRetirementTimelineRepository.FindPentionPayTimelines(businessGroup);
                    var payTimeline = new RetirementPayTimeline(timelines, member.Category, member.SchemeCode, _loggerFactory);
                    var payDay = payTimeline.PensionPayDay();
                    var timeToNormalRetirement = _retirementDatesService.GetFormattedTimeUntilNormalRetirement(member, retirementDatesAges, now);
                    var timeToTargetRetirement = _retirementDatesService.GetFormattedTimeUntilTargetRetirement(retirementDatesAges, now);

                    var transferCalculation = await _transferCalculationRepository.Find(businessGroup, referenceNumber);
                    var retirement = string.IsNullOrWhiteSpace(calc.RetirementJsonV2) ? null : _calculationsParser.GetRetirementV2(calc.RetirementJsonV2);

                    var builder = new CmsTokenInformationResponseBuilder()
                        .WithMemberData(member, retirementDate, now)
                        .WithRetirementDatesAges(retirementDatesAges, retirementDate, payDay, retirementExpirationDate)
                        .WithRetirementTime(timeToTargetRetirement, timeToNormalRetirement);

                    if (calc.IsCalculationSuccessful == false)
                        return Ok(builder.CalculationSuccessful(false)
                            .WithTransferQuoteData(transferCalculation.IsSome && transferCalculation.Value().TransferQuoteJson != null ? _calculationsParser.GetTransferQuote(transferCalculation.Value().TransferQuoteJson) : null)
                            .Build());

                    if (transferCalculation.IsNone || transferCalculation.Value().TransferQuoteJson == null)
                        return Ok(builder.CalculationSuccessful(true)
                            .WithRetirementV2Data(retirement, calc.RetirementJourney?.MemberQuote.Label, businessGroup, member.Scheme?.Type, submissionDate, calc.QuoteExpiryDate)
                            .Build());

                    var transferQuote = _calculationsParser.GetTransferQuote(transferCalculation.Value().TransferQuoteJson);
                    return Ok(builder.CalculationSuccessful(true)
                        .WithRetirementV2Data(retirement, calc.RetirementJourney?.MemberQuote.Label, businessGroup, member.Scheme?.Type, submissionDate, calc.QuoteExpiryDate)
                        .WithTransferQuoteData(transferQuote)
                        .Build());
                },
                async () =>
                {
                    var retirementDatesAgesResponse = await _calculationsClient.RetirementDatesAges(referenceNumber, businessGroup).Try();
                    string timeToNormalRetirement = null;
                    string timeToTargetRetirement = null;
                    Either<Error, InvestmentForecastResponse> investForecast = new InvestmentForecastResponse();
                    if (retirementDatesAgesResponse.IsSuccess)
                    {
                        var retirementDatesAges = new RetirementDatesAges(retirementDatesAgesResponse.Value());
                        timeToNormalRetirement = _retirementDatesService.GetFormattedTimeUntilNormalRetirement(member, retirementDatesAges, now);
                        timeToTargetRetirement = _retirementDatesService.GetFormattedTimeUntilTargetRetirement(retirementDatesAges, now);
                    }

                    return Ok(new CmsTokenInformationResponseBuilder()
                        .CalculationSuccessful(false)
                        .WithMemberData(member, null, DateTimeOffset.UtcNow)
                        .WithDirectRetirementDatesAgesResponseFromApi(retirementDatesAgesResponse)
                        .WithRetirementTime(timeToTargetRetirement, timeToNormalRetirement)
                        .Build());
                }
            );
    }

    [HttpGet]
    [Route("api/retirement/timeline")]
    [ProducesResponseType(typeof(RetirementTimelineResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> Timeline()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var now = DateTimeOffset.UtcNow;

        return await (await _calculationsRepository.Find(referenceNumber, businessGroup))
            .ToAsync()
            .MatchAsync<IActionResult>(
                async c =>
                {
                    var timelines = await _tenantRetirementTimelineRepository.Find(businessGroup);
                    var bankHolidays = await _bankHolidayRepository.ListFrom(c.EffectiveRetirementDate);
                    var retirement = _calculationsParser.GetRetirementV2(c.RetirementJsonV2);
                    var firstPayDates = new RetirementPayDates(timelines, bankHolidays, businessGroup, c.EffectiveRetirementDate);

                    return Ok(RetirementTimelineResponse.From(
                        c.EffectiveRetirementDate,
                        c.RetirementApplicationStartDateRange(now).EarliestStartRaDateForSelectedDate,
                        c.RetirementApplicationStartDateRange(now).LatestStartRaDateForSelectedDate,
                        c.RetirementConfirmationDate(),
                        firstPayDates.FirstMonthlyPensionPayDate(),
                        firstPayDates.LumpSumPayDate(retirement.IsAvc())));
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet]
    [Route("api/retirement/quote")]
    [ProducesResponseType(typeof(FileResult), 200, "application/octet-stream")]
    public async Task<IActionResult> GetRetirementQuote()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        var calculation = await _calculationsRepository.Find(referenceNumber, businessGroup);

        var EffectiveRetirementDate = new DateTime(2025, 6, 9);

        if (calculation.IsNone)
            return NotFound(ApiError.FromMessage("Calculation not found."));
        var retirementQuoteResult = await _calculationsClient.RetirementQuote(businessGroup, referenceNumber, calculation.Value().EffectiveRetirementDate);

        if (retirementQuoteResult.IsLeft)
            return BadRequest(ApiError.FromMessage(retirementQuoteResult.Left().Message));

        var awsResult = await _awsClient.File(retirementQuoteResult.Right().Item1);

        if (awsResult.IsLeft)
            return BadRequest(ApiError.FromMessage(awsResult.Left().Message));

        await _documentsUploaderService.UploadNonCaseRetirementQuoteDocument(businessGroup, referenceNumber, awsResult.Right(), retirementQuoteResult.Right().Item2);
        awsResult.Right().Position = 0;
        return File(awsResult.Right(), "application/pdf", "TransferApplicationSummary.pdf");
    }

    [HttpGet]
    [Route("api/retirement/dc/check-on-growth")]
    [ProducesResponseType(typeof(RateOfReturnResponse), 200)]
    [ProducesResponseType(typeof(NoContentResult), 204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GetRateOfReturn(DateTimeOffset startDate = default, DateTimeOffset effectiveDate = default)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var member = (await _memberRepository.FindMember(referenceNumber, businessGroup));
        if (member.IsNone)
        {
            _logger.LogError("Member not found for reference number {ReferenceNumber} and business group {BusinessGroup}", referenceNumber, businessGroup);
            return NotFound(ApiError.NotFound());
        }

        var result = await _rateOfReturnService.GetRateOfReturn(businessGroup, referenceNumber, startDate, effectiveDate);

        return result.Match<IActionResult>(
            Right: response =>
            {
                if (response.IsNone)
                {
                    return NoContent();
                }

                return Ok(response.Value());
            },
            Left: error => BadRequest(ApiError.FromMessage(error.Message))
        );
    }

    [HttpGet("api/retirement/db-core/application-status")]
    [ProducesResponseType(typeof(RetirementApplicationStatusResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GetDbCoreRetirementApplicationStatus(int preRetirementAgePeriod, int newlyRetiredRange, DateTime selectedDbCoreRetirementDate)
    {
        var now = DateTimeOffset.UtcNow;
        (string _, string referenceNumber, string businessGroup) = HttpContext.User.User();

        return await _memberRepository.FindMember(referenceNumber, businessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async member =>
                {
                    var calculation = await _calculationsRepository.Find(referenceNumber, businessGroup);
                    if (calculation.IsNone)
                    {
                        _logger.LogWarning("Calculation record does not exist. Unable to get proper db core application status and dates.");
                        return Ok(RetirementApplicationStatusResponse.From(RetirementApplicationStatus.Undefined, MemberLifeStage.Undefined));
                    }
                    var retirementDatesAges = _calculationsParser.GetRetirementDatesAges(calculation.Value().RetirementDatesAgesJson);
                    var journey = await _journeysRepository.Find(businessGroup, referenceNumber, "dbcoreretirementapplication");
                    if (journey.IsNone)
                        calculation.Value().UpdateEffectiveDate(selectedDbCoreRetirementDate);

                    return Ok(RetirementApplicationStatusResponse.From(
                            member.GetRetirementApplicationStatus(
                                now,
                                preRetirementAgePeriod,
                                newlyRetiredRange,
                                journey.Match(x => true, () => false),
                                journey.Match(x => x.SubmissionDate != null, () => false),
                                journey.Match(x => x.IsExpired(now), () => false),
                                calculation.Value().EffectiveRetirementDate,
                                retirementDatesAges
                            ),
                            calculation.Value().RetirementApplicationStartDateRange(now).EarliestStartRaDateForSelectedDate,
                            calculation.Value().RetirementApplicationStartDateRange(now).LatestStartRaDateForSelectedDate,
                            journey.MatchUnsafe(x => x.ExpirationDate, () => null),
                            member.GetLifeStage(now, preRetirementAgePeriod, newlyRetiredRange, retirementDatesAges)
                        ));
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet]
    [Route("api/retirement/latest-protected-quote")]
    [ProducesResponseType(typeof(LatestProtectedQuoteResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GetLatestProtectedQuote()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var now = DateTimeOffset.UtcNow;

        var member = await _memberRepository.FindMember(referenceNumber, businessGroup);
        if (member.IsNone)
            return NotFound(ApiError.NotFound());

        if (!_calculationsParser.IsGuaranteedQuoteEnabled(businessGroup))
            return BadRequest(ApiError.FromMessage("Guaranteed quote is not enabled for this business group."));

        var getGuaranteedQuoteClientRequest = new GetGuaranteedQuoteClientRequest
        {
            Bgroup = businessGroup,
            RefNo = referenceNumber,
            QuotationStatus = QuotationStatusValue.GUARANTEED.ToString(),
        };

        var guaranteedQuotes = await _calculationsClient.GetGuaranteedQuotes(getGuaranteedQuoteClientRequest);

        if (guaranteedQuotes.IsLeft)
            return Ok(LatestProtectedQuoteResponse.CalculationFailed());

        var latestProtectedQuote = guaranteedQuotes.Right().Quotations.Any()
            ? guaranteedQuotes.Right().Quotations.OrderByDescending(x => x.RunDate).First()
            : null;

        if (latestProtectedQuote?.EffectiveDate == null)
            return Ok(LatestProtectedQuoteResponse.CalculationFailed());

        var latestRetirementCalculation = await _calculationsClient.RetirementCalculationV2(referenceNumber, businessGroup, latestProtectedQuote.EffectiveDate.Value, false, false);

        if (latestRetirementCalculation.IsLeft)
            return Ok(LatestProtectedQuoteResponse.CalculationFailed());

        var calculation = await _calculationsRepository.Find(referenceNumber, businessGroup);
        if (calculation.IsSome)
        {
            var (retirementV2, mdp) = _calculationsParser.GetRetirementJsonV2(latestRetirementCalculation.Right().RetirementResponseV2, latestRetirementCalculation.Right().EventType);
            var (quoteGuaranteed, quoteExpiryDate) = _calculationsParser.GetGuaranteedQuoteDetail(latestRetirementCalculation.Right().RetirementResponseV2);

            calculation.Value().UpdateRetirementV2(
                retirementV2,
                mdp,
                latestProtectedQuote.EffectiveDate.Value.ToUniversalTime(),
                now,
                quoteGuaranteed,
                quoteExpiryDate
            );
            calculation.Value().UpdateCalculationSuccessStatus(true);

            await _mdpUnitOfWork.Commit();
        }
        else
        {
            var newCalculation = await NewCalculationV2(member.Value(), referenceNumber, businessGroup, now, false, latestProtectedQuote.EffectiveDate?.ToUniversalTime());
            if (newCalculation.IsSome)
            {
                await _calculationsRepository.Create(newCalculation.Value());
                await _mdpUnitOfWork.Commit();
            }
        }

        var latestRetirement = _calculationsParser.GetRetirementV2(_calculationsParser.GetRetirementJsonV2(latestRetirementCalculation.Right().RetirementResponseV2, latestRetirementCalculation.Right().EventType).Item1);

        var ageAtRetirementDateIso = default(string);
        if (latestProtectedQuote.EffectiveDate.HasValue && member.Value().PersonalDetails.DateOfBirth.HasValue)
        {
            var timePeriod = TimePeriodCalculator.Calculate(member.Value().PersonalDetails.DateOfBirth.Value.Date, latestProtectedQuote.EffectiveDate.Value.Date);
            var days = timePeriod.Weeks * 7 + timePeriod.Days;
            ageAtRetirementDateIso = $"P{timePeriod.Years}Y{timePeriod.month}M{days}D";
        }

        return Ok(new LatestProtectedQuoteResponse
        {
            QuoteExpiryDate = latestProtectedQuote.ExpiryDate,
            QuoteRetirementDate = latestProtectedQuote.EffectiveDate,
            AgeAtRetirementDateIso = ageAtRetirementDateIso,
            TotalPension = latestRetirement?.TotalPension(),
            TotalAVCFund = latestRetirement?.TotalAVCFund()
        });
    }
}