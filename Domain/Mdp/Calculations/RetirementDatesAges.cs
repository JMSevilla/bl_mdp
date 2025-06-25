using System;
using System.Collections.Generic;
using System.Linq;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.Web.Extensions;
using WTW.Web.Models.Internal;

namespace WTW.MdpService.Domain.Mdp.Calculations;

public class RetirementDatesAges
{
    public RetirementDatesAges(RetirementDatesAgesResponse retirementDatesAgesResponse)
    {
        NormalRetirementAge = retirementDatesAgesResponse.RetirementAges.NormalRetirementAge;
        NormalMinimumPensionAge = retirementDatesAgesResponse.RetirementAges.NormalMinimumPensionAge;
        NormalRetirementDate = retirementDatesAgesResponse.RetirementDates.NormalRetirementDate;
        EarliestRetirementAge = retirementDatesAgesResponse.RetirementAges.EarliestRetirementAge;
        LatestRetirementAge = retirementDatesAgesResponse.RetirementAges.LatestRetirementAge;
        EarliestRetirementDate = retirementDatesAgesResponse.RetirementDates.EarliestRetirementDate;
        NormalMinimumPensionDate = retirementDatesAgesResponse.RetirementDates.NormalMinimumPensionDate;
        LatestRetirementDate = retirementDatesAgesResponse.RetirementDates.LatestRetirementDate;
        TargetRetirementDate = retirementDatesAgesResponse.RetirementDates.TargetRetirementDate;
        TargetRetirementAgeIso = retirementDatesAgesResponse.RetirementAges.TargetRetirementAgeIso;
        TargetRetirementAgeYearsIso = retirementDatesAgesResponse.RetirementAges.TargetRetirementAgeYearsIso;
        WordingFlags = retirementDatesAgesResponse.WordingFlags;
        AgeAtNormalRetirementIso = retirementDatesAgesResponse.RetirementAges.AgeAtNormalRetirementIso;
    }

    public RetirementDatesAges(RetirementDatesAgesDto dto)
    {
        EarliestRetirementAge = dto.EarliestRetirementAge;
        NormalMinimumPensionAge = dto.NormalMinimumPensionAge;
        LatestRetirementAge = dto.LatestRetirementAge;
        NormalRetirementAge = dto.NormalRetirementAge;
        EarliestRetirementDate = dto.EarliestRetirementDate;
        NormalMinimumPensionDate = dto.NormalMinimumPensionDate;
        LatestRetirementDate = dto.LatestRetirementDate;
        NormalRetirementDate = dto.NormalRetirementDate;
        TargetRetirementDate = dto.TargetRetirementDate;
        TargetRetirementAgeIso = dto.TargetRetirementAgeIso;
        TargetRetirementAgeYearsIso = dto.TargetRetirementAgeYearsIso;
        WordingFlags = dto.WordingFlags ?? Enumerable.Empty<string>();
        AgeAtNormalRetirementIso = dto.AgeAtNormalRetirementIso;
    }

    public decimal EarliestRetirementAge { get; }
    public decimal? NormalMinimumPensionAge { get; }
    public decimal? LatestRetirementAge { get; }
    public decimal NormalRetirementAge { get; }
    public DateTimeOffset EarliestRetirementDate { get; }
    public DateTimeOffset? NormalMinimumPensionDate { get; }
    public DateTimeOffset? LatestRetirementDate { get; }
    public DateTimeOffset NormalRetirementDate { get; }
    public DateTimeOffset? TargetRetirementDate { get; }
    public string TargetRetirementAgeIso { get; }
    public string TargetRetirementAgeYearsIso { get; }
    public IEnumerable<string> WordingFlags { get; }
    public string AgeAtNormalRetirementIso { get; }

    public DateTime EarliestRetirementDateWithAppliedRetirementProcessingPeriod(int retirementProcessingPeriodInDays, DateTimeOffset utcNow)
    {
        var earliestEligibleRetirementDate = utcNow.AddDays(retirementProcessingPeriodInDays);
        var retirementDate = EarliestRetirementDate > utcNow ? EarliestRetirementDate : utcNow;

        if (retirementDate < earliestEligibleRetirementDate)
            return earliestEligibleRetirementDate.Date;

        return retirementDate.Date;
    }

    public DateTimeOffset LastAvailableQuoteDate(DateTimeOffset utcNow, BusinessGroup businessGroups)
    {
        var maximum = businessGroups.MaxQuoteWindowIsoDuration.ParseIsoDuration();
        var lastAvailableQuoteDate = utcNow.AddYears(maximum.Value.Years).AddMonths(maximum.Value.Months).AddDays(maximum.Value.Days);
        return lastAvailableQuoteDate < NormalRetirementDate ? lastAvailableQuoteDate : NormalRetirementDate;
    }

    public DateTimeOffset FirstAvailableQuoteDate(DateTimeOffset utcNow, BusinessGroup businessGroups)
    {
        var minQuoteWindow = businessGroups.MinQuoteWindowIsoDuration.ParseIsoDuration();
        var firstAvailableQuoteDate = utcNow.AddYears(minQuoteWindow.Value.Years).AddMonths(minQuoteWindow.Value.Months).AddDays(minQuoteWindow.Value.Days);
        return EarliestRetirementDate > firstAvailableQuoteDate ? EarliestRetirementDate : firstAvailableQuoteDate;
    }

    public DateTimeOffset EffectiveDate(DateTimeOffset utcNow, string businessGroup)
    {
        if (businessGroup.Equals("BCL", StringComparison.InvariantCultureIgnoreCase))
            return GetNormalRetirementDateForBarclaysMember(utcNow);

        return NormalRetirementDate > utcNow ? NormalRetirementDate : utcNow;
    }

    public int TargetRetirement()
    {
        return TargetRetirementAgeIso.ParseIsoDuration().Value.Years;
    }

    public int TargetRetirementYears()
    {
        return TargetRetirementAgeYearsIso.ParseIsoDuration().Value.Years;
    }

    public int NormalRetirement()
    {
        return decimal.ToInt32(NormalRetirementAge);
    }

    public int EarliestRetirement()
    {
        return decimal.ToInt32(EarliestRetirementAge);
    }

    public int? GetLatestRetirementAge()
    {
        if (!LatestRetirementAge.HasValue)
            return null;

        return decimal.ToInt32(LatestRetirementAge.Value);
    }

    public DateTime NormalRetirement(string businessGroup, DateTimeOffset utcNow, DateTime latestRetirementDate)
    {
        DateTimeOffset date;
        if (businessGroup.Equals("BCL", StringComparison.InvariantCultureIgnoreCase))
            date = GetNormalRetirementDateForBarclaysMember(utcNow);
        else
            date = NormalRetirementDate > utcNow ? NormalRetirementDate : utcNow;

        return date.Date > latestRetirementDate ? latestRetirementDate.Date : date.Date;
    }

    public string GetDCLifeStageStatus(DateTimeOffset utcNow)
    {
        return (TargetRetirementDate ?? NormalRetirementDate).Date switch
        {
            var date when date <= utcNow.Date => "exceededTRD",
            var date when date <= utcNow.Date.AddMonths(12) => "closeToTRD",
            var date when date <= utcNow.Date.AddMonths(36) => "approachingTRD",
            _ => "farFromTRD",
        };
    }

    private DateTimeOffset GetNormalRetirementDateForBarclaysMember(DateTimeOffset utcNow)
    {
        return NormalRetirementDate.AddMonths(-6) <= utcNow && NormalRetirementDate >= utcNow
            ? NormalRetirementDate
            : utcNow.AddMonths(6).AddDays(-1);
    }
}