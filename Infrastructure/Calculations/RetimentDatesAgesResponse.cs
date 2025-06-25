using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Calculations;

public record RetirementDatesAgesResponse
{
    public RetirementAgesResponse RetirementAges { get; init; }
    public RetirementDatesResponse RetirementDates { get; init; }
    public bool HasLockedInTransferQuote { get; set; }
    public int? LockedInTransferQuoteFileId { get; init; }
    public int? LockedInTransferQuoteImageId { get; init; }
    public int? LockedInTransferQuoteSeqno { get; init; }
    public IEnumerable<string> WordingFlags { get; set; }
}

public record RetirementAgesResponse
{
    public decimal EarliestRetirementAge { get; init; }
    public decimal? LatestRetirementAge { get; init; }
    public decimal NormalRetirementAge { get; init; }
    public decimal? NormalMinimumPensionAge { get; init; }

    [JsonPropertyName("target")]
    public string TargetRetirementAgeIso { get; init; }

    [JsonPropertyName("targetDerivedInteger")]
    public string TargetRetirementAgeYearsIso { get; init; }

    [JsonPropertyName("normal")]
    public string AgeAtNormalRetirementIso { get; init; }
}

public record RetirementDatesResponse
{
    public DateTimeOffset EarliestRetirementDate { get; init; }
    public DateTimeOffset? LatestRetirementDate { get; init; }
    public DateTimeOffset NormalRetirementDate { get; init; }
    public DateTimeOffset? NormalMinimumPensionDate { get; init; }

    [JsonPropertyName("target")]
    public DateTimeOffset? TargetRetirementDate { get; init; }
}