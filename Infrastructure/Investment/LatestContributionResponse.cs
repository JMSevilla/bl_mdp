using System;
using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.Investment;

public record LatestContributionResponse
{
    public decimal TotalValue { get; init; }
    public string Currency { get; init; }
    public DateTime? PaymentDate { get; init; }
    public List<Contribution> ContributionsList { get; init; }
}

public record Contribution
{
    public DateTime RegPayDate { get; init; }
    public string Name { get; init; }
    public decimal ContributionValue { get; init; }
}