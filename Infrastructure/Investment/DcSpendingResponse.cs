using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.Investment;

public record DcSpendingResponse<T> where T : class
{
    public List<T> ContributionTypes { get; init; }
}