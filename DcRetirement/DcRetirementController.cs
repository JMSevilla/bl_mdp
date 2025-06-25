using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WTW.MdpService.DcRetirement.Services;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.Investment;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.MdpService.Retirement;
using WTW.Web.Authorization;
using WTW.Web.Caching;
using WTW.Web.Errors;
using WTW.Web.Extensions;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.DcRetirement;

[ApiController]
public class DcRetirementController : ControllerBase
{
    private readonly IMemberRepository _memberRepository;
    private readonly ICalculationsRepository _calculationsRepository;
    private readonly IInvestmentServiceClient _investmentServiceClient;
    private readonly ICalculationsParser _calculationsParser;
    private readonly IDcRetirementService _dcRetirementService;
    private readonly IRetirementDatesService _retirementDatesService;
    private readonly ILogger<DcRetirementController> _logger;

    public DcRetirementController(IMemberRepository memberRepository,
        ICalculationsRepository calculationsRepository,
        IInvestmentServiceClient investmentServiceClient,
        ICalculationsParser calculationsParser,
        IDcRetirementService dcRetirementService,
        IRetirementDatesService retirementDatesService,
        ILogger<DcRetirementController> logger)
    {
        _memberRepository = memberRepository;
        _calculationsRepository = calculationsRepository;
        _investmentServiceClient = investmentServiceClient;
        _calculationsParser = calculationsParser;
        _dcRetirementService = dcRetirementService;
        _retirementDatesService = retirementDatesService;
        _logger = logger;
    }

    [HttpGet]
    [Route("api/retirement/dc-projected-balances")]
    [ProducesResponseType(typeof(DcRetirementProjectedBalancesResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GetDcProjectedBalances()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var now = DateTimeOffset.UtcNow;

        var member = (await _memberRepository.FindMember(referenceNumber, businessGroup)).Value();
        if (!member.IsSchemeDc())
        {
            _logger.LogWarning("Projected dc retirement balances are available only dor DC members. Bgroup: {bgroup}. Refno: {refno}.", businessGroup, referenceNumber);
            return BadRequest(ApiError.FromMessage("Projected dc retirement balances are available only dor DC members"));
        }

        return await _calculationsRepository.Find(referenceNumber, businessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async calculation =>
                {
                    var retirementDatesAges = _calculationsParser.GetRetirementDatesAges(calculation.RetirementDatesAgesJson);
                    var ageLines = _retirementDatesService.GetAgeLines(member.PersonalDetails, retirementDatesAges.TargetRetirementAgeYearsIso.ParseIsoDuration().Value.Years, now);
                    var investForecastResponse = await _investmentServiceClient.GetInvestmentForecast(referenceNumber, businessGroup, ageLines.Select(x => x.Age));
                    if (investForecastResponse.IsNone)
                    {
                        _logger.LogWarning("Investment forecast is not available for member {bgroup}:{refno}", businessGroup, referenceNumber);
                        return Problem(
                            title: "Internal Server Error",
                            detail: "Investment forecast is not available",
                            statusCode: StatusCodes.Status500InternalServerError
                            );
                    }

                    return Ok(new DcRetirementProjectedBalancesResponse(investForecastResponse.Value(), ageLines));
                },
                () =>
                {
                    _logger.LogWarning("Calculation is not present for member {bgroup}:{refno}", businessGroup, referenceNumber);
                    return NotFound(ApiError.NotFound());
                });
    }

    [HttpGet]
    [Route("api/retirement/dc/spending-strategies")]
    [ProducesResponseType(typeof(DcSpendingResponse<StrategyContributionTypeResponse>), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [CachedPerSession]
    public async Task<IActionResult> GetDcStrategies([FromQuery] InvestmentStrategiesRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        var targetSchemeMappingResponse = await _investmentServiceClient.GetTargetSchemeMappings(businessGroup, request.SchemeCode, request.Category);
        if (targetSchemeMappingResponse.IsNone)
            return BadRequest(ApiError.FromMessage("Failed to retrieve target scheme mappings"));

        var strategies = await _investmentServiceClient.GetInvestmentStrategies(targetSchemeMappingResponse.Value().Bgroup, targetSchemeMappingResponse.Value().SchemeCode, targetSchemeMappingResponse.Value().ContributionType);
        if (strategies.IsNone)
            return BadRequest(ApiError.FromMessage("Failed to retrieve investment strategies"));

        return Ok(strategies.Value());
    }

    [HttpGet]
    [Route("api/retirement/dc/spending-funds")]
    [ProducesResponseType(typeof(DcSpendingResponse<FundContributionTypeResponse>), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [CachedPerSession]
    public async Task<IActionResult> GetDcFunds([FromQuery] InvestmentFundsRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        var targetSchemeMappingResponse = await _investmentServiceClient.GetTargetSchemeMappings(businessGroup, request.SchemeCode, request.Category);
        if (targetSchemeMappingResponse.IsNone)
            return BadRequest(ApiError.FromMessage("Failed to retrieve target scheme mappings"));

        var funds = await _investmentServiceClient.GetInvestmentFunds(targetSchemeMappingResponse.Value().Bgroup, targetSchemeMappingResponse.Value().SchemeCode, targetSchemeMappingResponse.Value().ContributionType);
        if (funds.IsNone)
            return BadRequest(ApiError.FromMessage("Failed to retrieve investment funds"));

        return Ok(funds.Value());
    }

    [HttpPost]
    [Route("api/retirement/dc/reset-quote")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> ResetQuote()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        var result = await _dcRetirementService.ResetQuote(referenceNumber, businessGroup);
        if (result.HasValue)
            return BadRequest(ApiError.FromMessage(result.Value.Message));

        return NoContent();
    }

    [HttpGet]
    [Route("api/retirement/dc/lifesight-date-age")]
    [ProducesResponseType(typeof(LifeSighRetirementDateAgeResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GetLifeSightRetirementDateAge()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        return (await _investmentServiceClient.GetInvestmentForecastAge(referenceNumber, businessGroup))
            .Match<IActionResult>(
                x => Ok(new LifeSighRetirementDateAgeResponse(x)),
                () => NotFound(ApiError.NotFound()));
    }
}