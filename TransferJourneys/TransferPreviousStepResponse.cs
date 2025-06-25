namespace WTW.MdpService.TransferJourneys;

public record TransferPreviousStepResponse
{
    public string PreviousPageKey { get; init; }

    public static TransferPreviousStepResponse From(string previousPageKey)
    {
        return new()
        {
            PreviousPageKey = previousPageKey,
        };
    }
}