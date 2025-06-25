using System;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.RetirementJourneys;

public record PensionWiseResponse
{
    public DateTimeOffset? PensionWiseDate { get; init; }

    public static PensionWiseResponse From(RetirementJourney journey)
    {
        return new()
        {
            PensionWiseDate = journey.PensionWiseDate,
        };
    }
}