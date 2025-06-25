namespace WTW.MdpService.RetirementJourneys;
public record PageKeyResponse
{
    public string NextPageKey { get; init; }

    public static PageKeyResponse From(string nextPageKey)
    {
        return new()
        {
            NextPageKey = nextPageKey
        };
    }
}