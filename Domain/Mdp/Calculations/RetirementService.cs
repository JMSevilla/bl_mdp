using System.Collections.Generic;
using System.Linq;
using WTW.Web.Extensions;

namespace WTW.MdpService.Domain.Mdp.Calculations;

public class RetirementService : IRetirementService
{
    public Dictionary<string, object> GetSelectedQuoteDetails(string name, RetirementV2 retirement)
    {
        return AddOptionsToDictionary(GetSelectedQuotes(name, retirement.QuotesV2), retirement.TotalFundValue);
    }

    private Dictionary<string, object> AddOptionsToDictionary(IEnumerable<QuoteV2> selectedQuotes, decimal? totalFundValue)
    {
        var values = new Dictionary<string, object>();
        var quotes = selectedQuotes.ToArray();

        AddAdditionalRetirementValues(values, totalFundValue);

        foreach (var quote in quotes)
            values.Append<string, object>(AddOneOptionToDictionary(quote));

        return values;
    }

    private static Dictionary<string, object> AddOneOptionToDictionary(QuoteV2 selectedQuote)
    {
        var result = new Dictionary<string, object>();

        foreach (var attribute in selectedQuote.Attributes)
        {
            result.Add(selectedQuote.Name + "_" + attribute.Name, attribute.Value);
        }

        foreach (var tranche in selectedQuote.PensionTranches)
        {
            result.Add(selectedQuote.Name + "_pensionTranches_" + tranche.TrancheTypeCode, tranche.Value);
        }

        return result;
    }

    private void AddAdditionalRetirementValues(Dictionary<string, object> values, decimal? totalFundValue)
    {
        if (totalFundValue is not null)
            values.Add(nameof(totalFundValue), totalFundValue);
    }

    private IEnumerable<QuoteV2> GetSelectedQuotes(string name, IEnumerable<QuoteV2> quotes)
    {
        var quoteSubNames = name.Split(".").ToArray();
        var quotesNames = new List<string>();

        for (int i = 0; i < quoteSubNames.Length; i++)
        {
            quotesNames.Add(quoteSubNames.FullQuoteV2Name(i));
        }

        return quotes.Where(x => quotesNames.Contains(x.Name));
    }
}