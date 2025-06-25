using System.Collections.Generic;
using WTW.MdpService.Infrastructure.Investment;

namespace WTW.MdpService.DcRetirement;

public record FundContributionTypeResponse
{
    public string ContributionType { get; init; }
    public List<FundResponse> Funds { get; init; }
}