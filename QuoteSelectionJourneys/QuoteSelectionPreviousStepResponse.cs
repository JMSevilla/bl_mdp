namespace WTW.MdpService.QuoteSelectionJourneys;

public record QuoteSelectionPreviousStepResponse
{
    public string PreviousPageKey { get; init; }

    public static QuoteSelectionPreviousStepResponse From(string previousPageKey)
    {
        return new()
        {
            PreviousPageKey = previousPageKey,
        };
    }
}