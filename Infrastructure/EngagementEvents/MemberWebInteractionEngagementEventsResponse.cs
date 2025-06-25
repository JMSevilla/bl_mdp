using System;
using System.Collections.Generic;
using System.Linq;

namespace WTW.MdpService.Infrastructure.EngagementEvents;

public record MemberWebInteractionEngagementEventsResponse
{
    public IEnumerable<MemberEngagementEvent> Events { get; set; }
}

public record MemberEngagementEvent
{
    public string Event { get; set; } = null!;
    public DateTime Updated { get; set; }
    public string Status { get; set; } = null!;
    public decimal CompletionScore { get; set; }
    public decimal PercentComplete { get; set; }
    public DateTime LastUpdated { get; set; }
    public string LastStatus { get; set; } = null!;
    public DateTime NextUpdateDue { get; set; }
    public string NextStatus { get; set; } = null!;
    public decimal DisplayOrder { get; set; }
    public DateTime LastLogin { get; set; }
    public string UserId { get; set; } = null!;
}
