namespace WTW.MdpService.RetirementJourneys;
public record IntegrityResponse
{
    public string RedirectStepPageKey { get; init; }

    public static IntegrityResponse From(string redirectStepPageKey)
    {
        return new()
        {
            RedirectStepPageKey = redirectStepPageKey
        };
    }
}