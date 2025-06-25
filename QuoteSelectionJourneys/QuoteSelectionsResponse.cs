namespace WTW.MdpService.QuoteSelectionJourneys;

public record QuoteSelectionsResponse
{
    public string SelectedQuoteName { get; init; }

    public static QuoteSelectionsResponse From(string selectedQuoteName)
    {
        return new()
        {
            SelectedQuoteName = selectedQuoteName
        };
    }
}
