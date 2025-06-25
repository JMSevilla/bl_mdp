using System.Linq;

namespace WTW.MdpService.Infrastructure.Investment;

public static class InvestmentContributionFilter
{
    public static LatestContributionResponse Filter(LatestContributionResponse latestContribution)
    {
        var contributionList = latestContribution.ContributionsList;
        var positiveContributions = contributionList.Where(x => x.ContributionValue > 0).ToList();

        var filteredContributions = contributionList.Count switch
        {
            <= 2 => contributionList,
            > 2 when positiveContributions.Count > 2 => Enumerable.Empty<Contribution>(),
            > 2 when positiveContributions.Count <= 2 => positiveContributions
        };

        return new LatestContributionResponse
        {
            TotalValue = latestContribution.TotalValue,
            Currency = latestContribution.Currency,
            PaymentDate = latestContribution.PaymentDate,
            ContributionsList = filteredContributions.ToList()
        };
    }
}
