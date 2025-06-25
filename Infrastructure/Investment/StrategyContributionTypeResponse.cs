using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.Investment;

public record StrategyContributionTypeResponse
{
    public string ContributionType { get; init; }
    public List<StrategyResponse> Strategies { get; init; }
}