namespace WTW.MdpService.QuoteSelectionJourneys;

public record QuoteSelectionIntegrityResponse
{
    public string RedirectStepPageKey { get; init; }

    public static QuoteSelectionIntegrityResponse From(string redirectStepPageKey)
    {
        return new()
        {
            RedirectStepPageKey = redirectStepPageKey
        };
    }
}