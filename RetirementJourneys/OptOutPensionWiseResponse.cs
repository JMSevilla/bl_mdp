using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.RetirementJourneys;

public record OptOutPensionWiseResponse
{
    public bool? OptOutPensionWise { get; init; }

    public static OptOutPensionWiseResponse From(RetirementJourney journey)
    {
        return new()
        {
            OptOutPensionWise = journey.OptOutPensionWise,
        };
    }
}