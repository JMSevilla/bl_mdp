namespace WTW.MdpService.RetirementJourneys;
public record PreviousStepResponse
{
    public string PreviousPageKey { get; init; }

    public static PreviousStepResponse From(string previousPageKey)
    {
        return new()
        {
            PreviousPageKey = previousPageKey,
        };
    }
}