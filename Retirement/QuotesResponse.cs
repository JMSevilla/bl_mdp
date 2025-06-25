using System.Collections.Generic;
using System.Linq;

namespace WTW.MdpService.Retirement;

public record QuotesResponse
{
    public bool IsCalculationSuccessful { get; init; }
    public IEnumerable<QuoteResponse> Quotes { get; init; }
    public IEnumerable<string> WordingFlags { get; init; }
    public decimal? TotalAvcFundValue { get; init; }

    public static QuotesResponse CalculationFailed() => new() { IsCalculationSuccessful = false };

    public static QuotesResponse From(IEnumerable<Domain.Mdp.Calculations.Quote> quotes, IEnumerable<string> wordingFlags, decimal? totalAvcFundValue)
    {
        return new QuotesResponse
        {
            IsCalculationSuccessful = true,
            Quotes = quotes.Select(QuoteResponse.From),
            WordingFlags = wordingFlags,
            TotalAvcFundValue = totalAvcFundValue
        };
    }
}