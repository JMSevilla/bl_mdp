using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using WTW.MdpService.Infrastructure.Calculations;
using static WTW.MdpService.Infrastructure.Calculations.RetirementResponseV2;

namespace WTW.MdpService.Retirement;

public record QuotesResponseV2
{
    public bool IsCalculationSuccessful { get; init; }
    public MdpResponseV2 Quotes { get; init; }
    public IEnumerable<string> WordingFlags { get; init; }
    public decimal? TotalAvcFundValue { get; init; }

    public static QuotesResponseV2 CalculationFailed() => new() { IsCalculationSuccessful = false };

    public static QuotesResponseV2 From(MdpResponseV2 mdp, IEnumerable<string> wordingFlags, decimal? totalAvcFundValue)
    {
        return new QuotesResponseV2
        {
            IsCalculationSuccessful = true,
            Quotes = mdp,
            WordingFlags = wordingFlags,
            TotalAvcFundValue = totalAvcFundValue
        };
    }
}