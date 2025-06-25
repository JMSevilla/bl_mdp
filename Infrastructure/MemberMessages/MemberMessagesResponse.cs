using System;
using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.EngagementEvents;

public record MemberMessagesResponse
{
    public IEnumerable<MemberMessage> Messages { get; set; }
}

public record MemberMessage
{
    public int MessageNo { get; set; }
    public string MessageText { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string Title { get; set; }
}
