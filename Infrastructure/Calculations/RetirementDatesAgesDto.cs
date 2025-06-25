using System;
using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.Calculations;

public record RetirementDatesAgesDto
{
    public decimal EarliestRetirementAge { get; set; }
    public decimal? NormalMinimumPensionAge { get; set; }
    public decimal? LatestRetirementAge { get; set; }
    public decimal NormalRetirementAge { get; set; }
    public DateTimeOffset EarliestRetirementDate { get; set; }
    public DateTimeOffset? NormalMinimumPensionDate { get; set; }
    public DateTimeOffset? LatestRetirementDate { get; set; }
    public DateTimeOffset NormalRetirementDate { get; set; }
    public DateTimeOffset? TargetRetirementDate { get; set; }
    public string TargetRetirementAgeIso { get; set; }
    public string TargetRetirementAgeYearsIso { get; set; }
    public IEnumerable<string> WordingFlags { get; set; }
    public string AgeAtNormalRetirementIso { get; set; }
}