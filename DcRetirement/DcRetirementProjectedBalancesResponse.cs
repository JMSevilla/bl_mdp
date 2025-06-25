using System.Collections.Generic;
using System.Linq;
using WTW.MdpService.Infrastructure.Investment;
using WTW.MdpService.Retirement;

namespace WTW.MdpService.DcRetirement;

public record DcRetirementProjectedBalancesResponse
{
    public DcRetirementProjectedBalancesResponse(InvestmentForecastResponse investForecastResponse, IEnumerable<(int Age, bool IsTargetRetirementAge)> ageLines)
    {
        ProjectedBalances = ageLines.Select(x => new ProjectedBalance
        {
            Age = x.IsTargetRetirementAge ? x.Age.ToString() + "*" : x.Age.ToString(),
            AssetValue = investForecastResponse.Ages.SingleOrDefault(y => y.Age == x.Age)?.AssetValue,
        });
    }

    public IEnumerable<ProjectedBalance> ProjectedBalances { get; }
}