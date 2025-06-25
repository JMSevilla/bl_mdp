using System.Collections.Generic;

namespace WTW.MdpService.Retirement;

public record RecalculatedLumpSumQuoteResponse
{
    public bool IsCalculationSuccessful { get; init; }
    public QuoteResponse Quote { get; init; }
    public decimal? TotalAvcFundValue { get; init; }
    public IEnumerable<string> WordingFlags { get; init; }

    public static RecalculatedLumpSumQuoteResponse From(Domain.Mdp.Calculations.Quote quote, IEnumerable<string> wordingFlags, decimal? totalAvcFundValue)
    {
        return new RecalculatedLumpSumQuoteResponse
        {
            IsCalculationSuccessful = true,
            Quote = QuoteResponse.From(quote),
            TotalAvcFundValue = totalAvcFundValue,
            WordingFlags = wordingFlags
        };
    }

    public static RecalculatedLumpSumQuoteResponse CalculationFailed()
    {
        return new RecalculatedLumpSumQuoteResponse
        {
            IsCalculationSuccessful = true
        };
    }
}