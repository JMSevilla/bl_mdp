using System;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.TransferJourneys;

public record TransferPensionWiseResponse
{
    public TransferPensionWiseResponse(TransferJourney journey)
    {
        PensionWiseDate = journey.PensionWiseDate;
    }

    public DateTimeOffset? PensionWiseDate { get; init; }
}
