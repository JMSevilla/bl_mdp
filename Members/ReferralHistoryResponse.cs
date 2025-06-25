using System;
using System.Collections.Generic;
using System.Linq;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Members;

public record ReferralHistoryResponse
{
    public IEnumerable<ReferralHistoryItem> ReferralHistories { get; init; }
}

public record ReferralHistoryItem
{
    public string ReferralStatus { get; init; }
    public ReferralBadgeStatus ReferralBadgeStatus { get; init; }
    public DateTimeOffset? ReferralDate { get; init; }
}