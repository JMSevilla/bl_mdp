using System;
using System.Collections.Generic;
using System.Linq;
using WTW.MdpService.Infrastructure.EngagementEvents;

namespace WTW.MdpService.Members;

public record MemberAlertsResponse
{
    public IEnumerable<MemberAlert> Alerts { get; init; }

    public static MemberAlertsResponse From(MemberMessagesResponse messages)
    {
        return new()
        {
            Alerts = messages.Messages
                .Select(x => new MemberAlert
                {
                    AlertID = x.MessageNo,
                    MessageText = x.MessageText,
                    EffectiveDate = x.EffectiveDate
                })
        };
    }
}

public record MemberAlert
{
    public int AlertID { get; init; }
    public string MessageText { get; init; }
    public DateTime EffectiveDate { get; init; }
}
