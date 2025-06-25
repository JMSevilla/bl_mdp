using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.Investment;

public class InvestmentForecastResponse
{
    public List<InvestmentForecastByAgeResponse> Ages { get; set; } = new();
}