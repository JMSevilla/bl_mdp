using System.Collections.Generic;

namespace WTW.MdpService.Domain.Investment;

public class InvestmentForecastAge
{
    public InvestmentForecastAge(IEnumerable<InvestmentForecast> ages)
    {
        Ages = ages;
    }

    public IEnumerable<InvestmentForecast> Ages { get; }
}