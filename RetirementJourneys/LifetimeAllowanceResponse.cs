using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.RetirementJourneys;

public record LifetimeAllowanceResponse
{
    public decimal? Percentage { get; init; }

    public static LifetimeAllowanceResponse From(RetirementJourney journey)
    {
        return new()
        {
            Percentage = journey.ActiveLtaPercentage(),
        };
    }
}