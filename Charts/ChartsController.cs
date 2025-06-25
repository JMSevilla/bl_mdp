using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Infrastructure.Charts;
using WTW.MdpService.Infrastructure.Investment;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;
using WTW.Web.Serialization;

namespace WTW.MdpService.Charts;

[ApiController]
[Route("api/dc")]
public class ChartsController : ControllerBase
{
    private readonly IChartsTemporaryClient _chartsTemporaryClient;
    private readonly IInvestmentServiceClient _investmentServiceClient;
    private readonly IMemberRepository _memberRepository;
    private readonly ILogger<ChartsController> _logger;

    public ChartsController(
        IChartsTemporaryClient chartsTemporaryClient,
        IInvestmentServiceClient investmentServiceClient,
        IMemberRepository memberRepository,
        ILogger<ChartsController> logger)
    {
        _chartsTemporaryClient = chartsTemporaryClient;
        _investmentServiceClient = investmentServiceClient;
        _memberRepository = memberRepository;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet("charts/data")]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> GetChartData([FromQuery] ChartsRequest request) //with mocked data.
    {
#warning This is temp endpoint for testing charts in FE. Remove it afer Valentinas confirmation, that this is no longer needed.
#warning Also remove IChartsTemporaryClient interface.
        _logger.LogWarning("Using mocked data.Response same for all users.");
        var chartsData = await _chartsTemporaryClient.GetChartJsonData(request.TenantUrl);
        return Ok(chartsData);
    }

    [HttpGet("charts/investments")]
    [ProducesResponseType(typeof(IEnumerable<ChartsResponse>), 200)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> InvestmentChart([FromQuery] int numberOfDataItems = 4)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var member = (await _memberRepository.FindMember(referenceNumber, businessGroup));
        if (member.IsNone)
            return NotFound(ApiError.NotFound());

        var internalBalanceResponse = await _investmentServiceClient.GetInternalBalance(referenceNumber, businessGroup, member.Value().Scheme?.Type);
        if (internalBalanceResponse.IsNone)
        {
            return BadRequest(ApiError.FromMessage("Failed to retrieve investment data"));
        }

        _logger.LogWarning("Using mocked data from investment service api. Response same for all members.");

        var investmentInternalBalance = new InvestmentMappingService().MapResponseToDomain(internalBalanceResponse.Value());

        if (!investmentInternalBalance.Funds.Any())
            return NoContent();

        var chartData = investmentInternalBalance.MyInvestmentsChartData(numberOfDataItems);
        return Ok(new ChartsResponseBuilder().WithData(chartData).WithCurrency(investmentInternalBalance.Currency).Build());
    }

    [HttpGet("charts/total-paid-in")]
    [ProducesResponseType(typeof(IEnumerable<ChartsResponse>), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> TotalPaidInChart()
    {
        (string userId, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var member = (await _memberRepository.FindMember(referenceNumber, businessGroup));
        if (member.IsNone)
            return NotFound(ApiError.NotFound());

        _logger.LogWarning("Using mocked data from investment service api. Response same for all members.");

        var investmentInternalBalance = new InvestmentMappingService().MapResponseToDomain(GetInvestmentInternalBalance());
        var chartData = investmentInternalBalance.TotalPaidInChartData();

        return Ok(new ChartsResponseBuilder().WithData(chartData).Build());
    }

    [HttpGet("charts/contributions")]
    [ProducesResponseType(typeof(IEnumerable<ChartsResponse>), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> ContributionsCountChart()
    {
        (string userId, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var member = (await _memberRepository.FindMember(referenceNumber, businessGroup));
        if (member.IsNone)
            return NotFound(ApiError.NotFound());

        _logger.LogWarning("Using mocked data from investment service api. Response same for all members.");

        var investmentInternalBalance = new InvestmentMappingService().MapResponseToDomain(GetInvestmentInternalBalance());
        var chartData = investmentInternalBalance.ContributionsChartData();

        return Ok(new ChartsResponseBuilder().WithData(chartData).Build());
    }

    [HttpGet("charts/portfolio-performance")]
    [ProducesResponseType(typeof(IEnumerable<ChartsResponse>), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> PortfolioPerformanceChart() //with mocked data.
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var member = (await _memberRepository.FindMember(referenceNumber, businessGroup));
        if (member.IsNone)
            return NotFound(ApiError.NotFound());

        //call to calcApi POST rate-of-return
        _logger.LogWarning("Using hardcoded values. Response same for all members.");

        return Ok(new ChartsResponseBuilder().WithDataSetPortfolio().Build());
    }

    /// <summary>
    /// Created only for testing purposes.
    /// </summary>
    /// <returns></returns>
    private static InvestmentInternalBalanceResponse GetInvestmentInternalBalance()
    {
          var balanceResponseJson = @"{
              ""currency"": ""GBP"",
              ""totalPaidIn"": 33045.64,
              ""totalValue"": 32642.63,
              ""funds"": [
                {
                  ""code"": ""LSBO"",
                  ""name"": ""Lifesight diversified growth"",
                  ""value"": 4899.07
                },
                {
                  ""code"": ""LSCA"",
                  ""name"": ""UK Index"",
                  ""value"": 14273.28
                },
                {
                  ""code"": ""LSDG"",
                  ""name"": ""Property"",
                  ""value"": 13470.28
                },
                {
                  ""code"": ""LSDG"",
                  ""name"": ""LSDG"",
                  ""value"": 1370.28
                }
              ],
              ""contributions"": [
                {
                  ""code"": ""HIST"",
                  ""name"": ""Bulk transfer"",
                  ""paidIn"": 33045.64,
                  ""value"": 32642.63
                }
              ]
            }";

        return JsonSerializer.Deserialize<InvestmentInternalBalanceResponse>(balanceResponseJson, SerialiationBuilder.Options());

    }
}