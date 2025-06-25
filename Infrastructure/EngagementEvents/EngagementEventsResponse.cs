using System.Collections.Generic;
using System.Linq;

namespace WTW.MdpService.Infrastructure.EngagementEvents;

public record EngagementEventsResponse
{
    public IEnumerable<EngagementEvent> Events { get; init; }

    public static EngagementEventsResponse From(MemberWebInteractionEngagementEventsResponse engagementEvents)
    {
        return new()
        {
            Events = engagementEvents.Events.Select(x => new EngagementEvent
            {
                Event = x.Event,
                Status = x.Status switch
                {
                    "COMPLETE" => x.Status,
                    _ => x.LastStatus == "COMPLETE" ? "REVIEW" : "INCOMPLETE"
                }
            })
        };
    }
}

public record EngagementEvent
{
    public string Event { get; init; }
    public string Status { get; init; }
}
