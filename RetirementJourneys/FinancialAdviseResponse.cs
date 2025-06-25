using System;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.RetirementJourneys;

public record FinancialAdviseResponse
{
    public DateTimeOffset? FinancialAdviseDate { get; init; }

    public static FinancialAdviseResponse From(RetirementJourney journey)
    {
        return new()
        {
            FinancialAdviseDate = journey.FinancialAdviseDate,
        };
    }
}