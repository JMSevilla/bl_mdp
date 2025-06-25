using System;
using System.Collections.Generic;
using System.Linq;

namespace WTW.MdpService.Investment;

public record LatestContributionResponse()
{
    public decimal? TotalValue { get; init; }
    public string? Currency { get; init; }
    public DateTime? PaymentDate { get; init; }
    public List<Contribution> Contributions { get; init; } = new();

    public static LatestContributionResponse From(Infrastructure.Investment.LatestContributionResponse response)
    {
        return new()
        {
            TotalValue = response.TotalValue,
            Currency = response.Currency,
            PaymentDate = response.PaymentDate,
            Contributions = response.ContributionsList.Select(x => new Contribution(x.Name, x.ContributionValue)).ToList()
        };
    }
}

public record Contribution(string Label, decimal Value);