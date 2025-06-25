namespace WTW.MdpService.Journeys;

public record JourneyStageRequest
{
    public string Stage { get; init; }
    public JourneyStageStatusPageRequest Page { get; init; }
}
