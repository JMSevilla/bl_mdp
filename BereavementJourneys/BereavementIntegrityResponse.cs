namespace WTW.MdpService.BereavementJourneys;

public record BereavementIntegrityResponse
{
    public string RedirectStepPageKey { get; init; }

    public static BereavementIntegrityResponse From(string redirectStepPageKey)
    {
        return new()
        {
            RedirectStepPageKey = redirectStepPageKey
        };
    }
}