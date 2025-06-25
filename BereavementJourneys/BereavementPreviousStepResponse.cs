namespace WTW.MdpService.BereavementJourneys;

public record BereavementPreviousStepResponse
{
    public string PreviousPageKey { get; init; }

    public static BereavementPreviousStepResponse From(string previousPageKey)
    {
        return new()
        {
            PreviousPageKey = previousPageKey,
        };
    }
}