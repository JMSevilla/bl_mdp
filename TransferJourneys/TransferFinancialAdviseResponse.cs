using System;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.TransferJourneys;

public record TransferFinancialAdviseResponse
{
    public TransferFinancialAdviseResponse(TransferJourney journey)
    {
        FinancialAdviseDate = journey.FinancialAdviseDate;
    }
    public DateTimeOffset? FinancialAdviseDate { get; init; }
}