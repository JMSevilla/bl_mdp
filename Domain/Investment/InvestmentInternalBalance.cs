using System;
using System.Collections.Generic;
using System.Linq;

namespace WTW.MdpService.Domain.Investment;

public class InvestmentInternalBalance
{
    public InvestmentInternalBalance(
        string currency,
        decimal totalPaidIn,
        decimal totalValue,
        IEnumerable<InvestmentContributionsData> contributions,
        IEnumerable<InvestmentFund> funds)
    {
        Currency = currency;
        TotalPaidIn = totalPaidIn;
        TotalValue = totalValue;
        Contributions = contributions;
        Funds = funds;
    }

    public string Currency { get; }
    public decimal TotalPaidIn { get; }
    public decimal TotalValue { get; }
    public IEnumerable<InvestmentContributionsData> Contributions { get; }
    public IEnumerable<InvestmentFund> Funds { get; }

    public IEnumerable<InvestmentFundChartData> MyInvestmentsChartData(int numberOfDataItems)
    {
        var fundsToDisplay = new List<InvestmentFundChartData>();
        foreach (var fund in Funds)
        {
            fundsToDisplay.Add(new InvestmentFundChartData { Label = fund.Name, Value = fund.Value });
        }

        var orderedFunds = fundsToDisplay.OrderByDescending(x => x.Value).ToList();
        List<InvestmentFundChartData> result;

        if (orderedFunds.Count <= numberOfDataItems)
        {
            result = orderedFunds.Take(numberOfDataItems).ToList();
            return result;
        }

        result = orderedFunds.Take(numberOfDataItems - 1).ToList();
        var otherFundsValuesSum = orderedFunds.Except(result).Sum(x => x.Value);
        result.Add(new InvestmentFundChartData { Label = "Other", Value = otherFundsValuesSum });

        return result;
    }

    public IEnumerable<InvestmentFundChartData> TotalPaidInChartData()
    {
        var result = new List<InvestmentFundChartData>();
        result.AddRange(
            new List<InvestmentFundChartData>
            {
                new InvestmentFundChartData { Label = "Current value", Value = TotalValue },
                new InvestmentFundChartData { Label = "Total paid in", Value = TotalPaidIn }
            });

        return result;
    }

    public IEnumerable<InvestmentFundChartData> ContributionsChartData()
    {
        var contributionsResult = new List<InvestmentFundChartData>();
        var totalPaidInValue = Contributions.Sum(x => x.PaidIn);
        foreach(var item in Contributions)
        {
            var percentage = Math.Round(((item.PaidIn / totalPaidInValue) * 100), 2);
            contributionsResult.Add(new InvestmentFundChartData { Label = item.Name, Value = percentage });
        }

        var orderedContributions = contributionsResult.OrderByDescending(x => x.Value).ToList();
        var result = orderedContributions.Take(2).ToList();
        var otherContributionsValuesSum = orderedContributions.Except(result).Sum(x => x.Value);
        result.Add(new InvestmentFundChartData { Label = "Other", Value = otherContributionsValuesSum });

        return result;
    }
}