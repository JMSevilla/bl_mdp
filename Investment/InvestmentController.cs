using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.Investment;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Investment;

[ApiController]
[Route("api/investment")]
public class InvestmentController : ControllerBase
{
    private readonly IMemberRepository _memberRepository;
    private readonly IInvestmentServiceClient _investmentServiceClient;
    private readonly ICalculationsRepository _calculationsRepository;
    private readonly ICalculationsParser _calculationsParser;
    private readonly ILogger<InvestmentController> _logger;
    private readonly IInvestmentQuoteService _investmentQuoteService;

    public InvestmentController(
        IInvestmentServiceClient investmentServiceClient,
        IMemberRepository memberRepository,
        ICalculationsRepository calculationsRepository,
        ICalculationsParser calculationsParser,
        ILogger<InvestmentController> logger,
        IInvestmentQuoteService investmentQuoteService)
    {
        _investmentServiceClient = investmentServiceClient;
        _memberRepository = memberRepository;
        _calculationsRepository = calculationsRepository;
        _calculationsParser = calculationsParser;
        _logger = logger;
        _investmentQuoteService = investmentQuoteService;
    }

    [HttpGet("internal-balance")]
    [ProducesResponseType(typeof(InternalBalanceResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GetInternalBalance()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var member = await _memberRepository.FindMember(referenceNumber, businessGroup);
        if (member.IsNone)
        {
            _logger.LogError("Member not found. Business Group: {businessGroup}. Reference Number: {referenceNumber}.", businessGroup, referenceNumber);
            return NotFound(ApiError.FromMessage("Member not found."));
        }

        var internalBalance = await _investmentServiceClient.GetInternalBalance(referenceNumber, businessGroup, member.Value().Scheme?.Type);
        return Ok(new InternalBalanceResponse(internalBalance));
    }

    [HttpGet("forecast")]
    [ProducesResponseType(typeof(ForecastResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GetInvestmentForecast()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var member = await _memberRepository.FindMember(referenceNumber, businessGroup);
        if (member.IsNone)
        {
            _logger.LogError("Member not found. Business Group: {businessGroup}. Reference Number: {referenceNumber}.", businessGroup, referenceNumber);
            return NotFound(ApiError.FromMessage("Member not found."));
        }

        var calculation = await _calculationsRepository.Find(referenceNumber, businessGroup);
        if (calculation.IsNone)
        {
            _logger.LogWarning("No calculation found for {referenceNumber} {businessGroup}", referenceNumber, businessGroup);
            return Ok(new ForecastResponse(null));
        }

        var retirementDatesAges = _calculationsParser.GetRetirementDatesAges(calculation.Value().RetirementDatesAgesJson);
        var investForecastResponse = await _investmentServiceClient.GetInvestmentForecast(
            referenceNumber,
            businessGroup,
            retirementDatesAges.TargetRetirementYears(),
            member.Value().Scheme?.Type);

        return Ok(new ForecastResponse(investForecastResponse.Right()));
    }

    [HttpGet("latest-contribution")]
    [ProducesResponseType(typeof(LatestContributionResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> GetLatestContribution()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        if (!(await _memberRepository.ExistsMember(referenceNumber, businessGroup)))
        {
            _logger.LogError("Member not found. Business Group: {businessGroup}. Reference Number: {referenceNumber}.", businessGroup, referenceNumber);
            return NotFound(ApiError.FromMessage("Member not found."));
        }

        var latestContribution = await _investmentServiceClient.GetLatestContribution(businessGroup, referenceNumber);
        if (latestContribution.IsNone)
        {
            _logger.LogInformation("Failed to retrieve latest contribution for {businessGroup} {referenceNumber}", businessGroup, referenceNumber);
            return Problem(
                          title: "Internal Server Error",
                          detail: "Failed to retrieve latest contribution.",
                          statusCode: StatusCodes.Status500InternalServerError
                          );
        }

        if (!latestContribution.Value().ContributionsList.Any())
        {
            _logger.LogInformation("Member has no contributions.");
            return NoContent();
        }

        var filteredContributions = InvestmentContributionFilter.Filter(latestContribution.Value());

        return Ok(LatestContributionResponse.From(filteredContributions));
    }

    [HttpPost("annuity/quote")]
    [ProducesResponseType(typeof(ApiError), 500)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> CreateAnnuityQuote()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        return await _calculationsRepository.Find(referenceNumber, businessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async calculation =>
                {
                    var retirement = _calculationsParser.GetRetirementV2(calculation.RetirementJsonV2);

                    var annuitiyQuoteRequest = await _investmentQuoteService.CreateAnnuityQuoteRequest(
                        businessGroup,
                        referenceNumber,
                        retirement);

                    if (annuitiyQuoteRequest.IsLeft)
                    {
                        _logger.LogError("Failed to get member details. Error: {error}", annuitiyQuoteRequest.Left().Message);
                        return BadRequest(ApiError.FromMessage("Failed to create annuity quote."));
                    }

                    var createQuote = await _investmentServiceClient.CreateAnnuityQuote(
                        businessGroup,
                        referenceNumber,
                        annuitiyQuoteRequest.Right());

                    if (createQuote.IsLeft)
                    {
                        _logger.LogError("Failed to create annuity quote for {businessGroup} {referenceNumber}. Error: {error}", businessGroup, referenceNumber, createQuote.Left().Message);
                        return Problem(detail: "Failed to create annuity quote", statusCode: StatusCodes.Status500InternalServerError, title: "Internal Server Error");
                    }

                    return NoContent();
                },
                () =>
                {
                    _logger.LogWarning("No calculation found for {referenceNumber} {businessGroup}", referenceNumber, businessGroup);
                    return BadRequest(ApiError.FromMessage("No calculation found."));
                });
    }
}