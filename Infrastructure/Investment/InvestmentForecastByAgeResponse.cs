namespace WTW.MdpService.Infrastructure.Investment;

public class InvestmentForecastByAgeResponse
{
    public int Age { get; set; }
    public decimal? AssetValue { get; set; }
    public decimal? TaxFreeCash { get; set; }
}
