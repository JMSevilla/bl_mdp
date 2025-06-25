namespace WTW.MdpService.TransferJourneys;

public record TransferIntegrityResponse
{
    public string RedirectStepPageKey { get; init; }

    public static TransferIntegrityResponse From(string redirectStepPageKey)
    {
        return new()
        {
            RedirectStepPageKey = redirectStepPageKey
        };
    }
}