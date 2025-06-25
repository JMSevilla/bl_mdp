using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Content.V2;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.MdpService.Infrastructure.MemberWebInteractionService;
using WTW.MdpService.RetirementJourneys;
using WTW.Web.Authorization;
using WTW.Web.Caching;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Members;

[ApiController]
[Route("api/members")]
public class MemberController : ControllerBase
{
    private readonly ICalculationsClient _calculationsClient;
    private readonly IMemberRepository _memberRepository;
    private readonly RetirementJourneyConfiguration _retirementJourneyConfiguration;
    private readonly IIfaReferralHistoryRepository _ifaReferralHistoryRepository;
    private readonly ICalculationsParser _calculationsParser;
    private readonly ICache _cache;
    private readonly ILogger<MemberController> _logger;
    private readonly ICalculationsRepository _calculationsRepository;
    private readonly IAccessKeyService _accessKeyService;
    private readonly IMemberWebInteractionServiceClient _memberWebInteractionServiceClient;

    public MemberController(
        ICalculationsClient calculationsClient,
        IMemberRepository memberRepository,
        ICalculationsRepository calculationsRepository,
        RetirementJourneyConfiguration retirementJourneyConfiguration,
        IIfaReferralHistoryRepository ifaReferralHistoryRepository,
        ICalculationsParser calculationsParser,
        ICache cache,
        ILogger<MemberController> logger,
        IAccessKeyService accessKeyService,
        IMemberWebInteractionServiceClient memberWebInteractionServiceClient)
    {
        _calculationsClient = calculationsClient;
        _memberRepository = memberRepository;
        _calculationsRepository = calculationsRepository;
        _retirementJourneyConfiguration = retirementJourneyConfiguration;
        _ifaReferralHistoryRepository = ifaReferralHistoryRepository;
        _calculationsParser = calculationsParser;
        _cache = cache;
        _logger = logger;
        _accessKeyService = accessKeyService;
        _memberWebInteractionServiceClient = memberWebInteractionServiceClient;
    }

    [HttpGet("current/scheme")]
    [ProducesResponseType(typeof(SchemeResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GetScheme()
    {
        (string _, string referenceNumber, string businessGroup) = HttpContext.User.User();
        _logger.LogInformation("Retrieving {businessGroup}:{referenceNumber} scheme details.", businessGroup, referenceNumber);
        return await _memberRepository.FindMember(referenceNumber, businessGroup)
            .ToAsync()
            .Match<IActionResult>(
                member => Ok(new SchemeResponse(member)),
                () =>
                {
                    _logger.LogWarning("Member {businessGroup}:{referenceNumber} is not found.", businessGroup, referenceNumber);
                    return NotFound(ApiError.NotFound());
                });
    }

    [HttpGet("current/age-lines")]
    [ProducesResponseType(typeof(AgeLinesResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GetAgeLines()
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
                        return NotFound(ApiError.FromMessage("Calculation not found."));

                    var retirementDatesAges = _calculationsParser.GetRetirementDatesAges(calculation.Value().RetirementDatesAgesJson);

                    return Ok(AgeLinesResponse.From(
                        member.GetAgeLines(now, retirementDatesAges.EarliestRetirement(), retirementDatesAges.NormalRetirement())
                    ));
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("current/retirement-application-status-v2")]
    [ProducesResponseType(typeof(RetirementApplicationStatusResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GetRetirementApplicationStatusV2(int preRetirementAgePeriod, int newlyRetiredRange)
    {
        var now = DateTimeOffset.UtcNow;
        (string _, string referenceNumber, string businessGroup) = HttpContext.User.User();

        return await _memberRepository.FindMember(referenceNumber, businessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async member =>
                {
                    var calculation = await _calculationsRepository.Find(referenceNumber, businessGroup);
                    if (calculation.IsNone || calculation.Value().IsCalculationSuccessful == false)
                        return Ok(RetirementApplicationStatusResponse.From(RetirementApplicationStatus.Undefined, MemberLifeStage.Undefined));

                    var retirementDatesAges = _calculationsParser.GetRetirementDatesAges(calculation.Value().RetirementDatesAgesJson);

                    try
                    {
                        return Ok(RetirementApplicationStatusResponse.From
                            (
                                member.GetRetirementApplicationStatus(
                                    now,
                                    preRetirementAgePeriod,
                                    newlyRetiredRange,
                                    calculation.Value().HasRetirementJourneyStarted(),
                                    calculation.Value().IsRetirementJourneySubmitted(),
                                    calculation.Value().HasRetirementJourneyExpired(now),
                                    calculation.Value().EffectiveRetirementDate,
                                    retirementDatesAges
                                ),
                                calculation.Value().RetirementApplicationStartDateRange(now).EarliestStartRaDateForSelectedDate,
                                calculation.Value().RetirementApplicationStartDateRange(now).LatestStartRaDateForSelectedDate,
                                calculation
                                    .Value()
                                    .ExpectedRetirementJourneyExpirationDate(now, _retirementJourneyConfiguration.RetirementJourneyDaysToExpire),
                                member.GetLifeStage(now, preRetirementAgePeriod, newlyRetiredRange, retirementDatesAges)
                            )
                        );
                    }
                    catch (Exception)
                    {
                        return Ok(RetirementApplicationStatusResponse.From(
                            RetirementApplicationStatus.Undefined,
                            member.GetLifeStage(now, preRetirementAgePeriod, newlyRetiredRange, retirementDatesAges)));
                    }
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("analytics")]
    [ProducesResponseType(typeof(MemberAnalyticsResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GetMemberAnalyticsDetails([FromQuery] string contentAccessKey)
    {
        return await _memberRepository.FindMember(HttpContext.User.User().ReferenceNumber, HttpContext.User.User().BusinessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(async member =>
            {
                var accessKey = _accessKeyService.ParseJsonToAccessKey(contentAccessKey);
                var userId = HttpContext.User.User().UserId;
                var dcJourneyStatus = _accessKeyService.GetDcJourneyStatus(accessKey.WordingFlags);

                if (await _calculationsRepository.Find(HttpContext.User.User().ReferenceNumber, HttpContext.User.User().BusinessGroup) is var calculation
                    && calculation.IsSome && !string.IsNullOrWhiteSpace(calculation.Value().RetirementJsonV2))
                {
                    var retirement = _calculationsParser.GetRetirementV2(calculation.Value().RetirementJsonV2);
                    var retirementDatesAges = _calculationsParser.GetRetirementDatesAges(calculation.Value().RetirementDatesAgesJson);
                    return Ok(MemberAnalyticsResponse.From(member, retirement.IsAvc(), retirementDatesAges, userId, accessKey, dcJourneyStatus));
                }

                return Ok(MemberAnalyticsResponse.From(member, false, null, userId, accessKey, dcJourneyStatus));

            },
            async () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("personal-details")]
    [ProducesResponseType(typeof(MemberPersonalDetailsResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GetPersonalDetails()
    {
        return (await _memberRepository.FindMember(HttpContext.User.User().ReferenceNumber, HttpContext.User.User().BusinessGroup))
            .Match<IActionResult>(
                member => Ok(MemberPersonalDetailsResponse.From(member.PersonalDetails, member.InsuranceNumber)),
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("current/time-to-retirement")]
    [ProducesResponseType(typeof(TimePeriodResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GetTimeToRetirement()
    {
        (string _, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var calculation = await _calculationsRepository.Find(referenceNumber, businessGroup);

        if (calculation.IsNone)
            return NotFound(ApiError.NotFound());

        var retirementDatesAges = _calculationsParser.GetRetirementDatesAges(calculation.Value().RetirementDatesAgesJson);
        var now = DateTimeOffset.UtcNow;

        return await _memberRepository.FindMember(referenceNumber, businessGroup)
            .ToAsync()
            .Match<IActionResult>(
                m =>
                {
                    var latestRetirementDate = m.LatestRetirementDate(retirementDatesAges.LatestRetirementDate, retirementDatesAges.GetLatestRetirementAge(), businessGroup, now);
                    var normalRetirementDate = retirementDatesAges.NormalRetirement(businessGroup, now, latestRetirementDate).Date;
                    return Ok(TimePeriodResponse.From(
                        TimePeriodCalculator.Calculate(now.Date, normalRetirementDate),
                        TimePeriodCalculator.Calculate(m.PersonalDetails.DateOfBirth.Value.Date,
                            businessGroup.Equals("BCL", StringComparison.InvariantCultureIgnoreCase) ? normalRetirementDate : retirementDatesAges.NormalRetirementDate.Date),
                        TimePeriodCalculator.Calculate(m.PersonalDetails.DateOfBirth.Value.Date, now.Date)));
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("current/membership-summary")]
    [ProducesResponseType(typeof(MembershipSummaryResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GetMembershipSummary()
    {
        (string _, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var cacheKey = MembershipSummary.CacheKey(businessGroup, referenceNumber);
        var cacheMembershipResponse = await _cache.Get<MembershipSummaryResponse>(cacheKey);
        if (cacheMembershipResponse.IsSome)
            return Ok(cacheMembershipResponse.Value());

        return await _memberRepository.FindMember(referenceNumber, businessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async m =>
                {
                    var calculation = await _calculationsRepository.Find(referenceNumber, businessGroup);

                    if (calculation.IsNone || string.IsNullOrWhiteSpace(calculation.Value().RetirementJsonV2))
                    {
                        var retirementDatesAgesResponse = await _calculationsClient.RetirementDatesAges(referenceNumber, businessGroup).Try();
                        return Ok(MembershipSummaryResponse.From(new MembershipSummary(m, retirementDatesAgesResponse, DateTimeOffset.UtcNow)));
                    }

                    var retirementDatesAges = _calculationsParser.GetRetirementDatesAges(calculation.Value().RetirementDatesAgesJson);
                    var retirement = _calculationsParser.GetRetirementV2(calculation.Value().RetirementJsonV2);

                    var response = MembershipSummaryResponse.From(new MembershipSummary(m, retirementDatesAges, retirement, DateTimeOffset.UtcNow));
                    await _cache.Set<MembershipSummaryResponse>(cacheKey, response);
                    return Ok(response);
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("linked-members")]
    [ProducesResponseType(typeof(LinkedMemberResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> LinkedMembers()
    {
        return await _memberRepository.FindLinkedMembers(HttpContext.User.UserWithLinkedMember().MainReferenceNumber, HttpContext.User.UserWithLinkedMember().MainBusinessGroup,
                HttpContext.User.UserWithLinkedMember().ReferenceNumber, HttpContext.User.UserWithLinkedMember().BusinessGroup)
            .ToAsync()
            .Match<IActionResult>(
                m => Ok(LinkedMemberResponse.From(m)),
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("current/referral-history")]
    [ProducesResponseType(typeof(ReferralHistoryResponse), 200)]
    public async Task<IActionResult> ReferralHistory()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        var ifaReferralHistory = await _ifaReferralHistoryRepository.Find(referenceNumber, businessGroup);

        var builder = new ReferralHistoryItemsBuilder(ifaReferralHistory);
        return Ok(new ReferralHistoryResponse { ReferralHistories = builder.ReferralHistoryItems() });
    }

    [HttpGet("current/dependants")]
    [ProducesResponseType(typeof(DependantsResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> Dependats()
    {
        return await _memberRepository.FindMemberWithDependant(HttpContext.User.User().ReferenceNumber, HttpContext.User.User().BusinessGroup)
            .ToAsync()
            .Match<IActionResult>(
                m => Ok(DependantsResponse.From(m.Dependants)),
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("alerts")]
    [ProducesResponseType(typeof(MemberAlertsResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GetAlerts()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var member = await _memberRepository.FindMember(referenceNumber, businessGroup);
        if (member.IsNone)
        {
            _logger.LogError("Member not found for reference number {referenceNumber} and business group {businessGroup}", referenceNumber, businessGroup);
            return NotFound(ApiError.NotFound());
        }

        var memberMessages = await _memberWebInteractionServiceClient.GetMessages(businessGroup, referenceNumber);
        if (memberMessages.IsNone)
        {
            _logger.LogInformation("No alerts found for {businessGroup} {referenceNumber}", businessGroup, referenceNumber);
            return NoContent();
        }

        var memberAlerts = MemberAlertsResponse.From(memberMessages.Value());

        _logger.LogInformation("Alerts returned successfully for {businessGroup} {referenceNumber}", businessGroup, referenceNumber);

        return Ok(memberAlerts);
    }
}