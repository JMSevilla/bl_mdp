using System.Collections.Generic;

namespace WTW.MdpService.Journeys;

public record JourneyStageStatusPageRequest
{
    public IEnumerable<string> Start { get; init; }
    public IEnumerable<string> End { get; init; }
}