using System.Linq;
using WTW.MdpService.Infrastructure.Investment;

namespace WTW.MdpService.Investment;

public class ForecastResponse
{
    public ForecastResponse(InvestmentForecastResponse investmentForecast)
    {
        DcProjectedBalance = investmentForecast?.Ages?.FirstOrDefault()?.AssetValue;
    }

    public decimal? DcProjectedBalance { get; set; }
}
