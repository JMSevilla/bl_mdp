namespace WTW.MdpService.Infrastructure.Investment;

public record InvestmentForecastAgeResponse
{
    public string RetirementDate { get; set; }
    public int RetirementAge { get; set; }
}