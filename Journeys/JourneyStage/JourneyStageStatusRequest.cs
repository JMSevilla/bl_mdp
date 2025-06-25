using System.Collections.Generic;

namespace WTW.MdpService.Journeys;

public record JourneyStageStatusRequest
{
    public IEnumerable<JourneyStageRequest> Stages { get; init; }
}