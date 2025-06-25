using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt.Common;
using LanguageExt.UnsafeValueAccess;
using WTW.Web.Extensions;

namespace WTW.MdpService.Domain.Mdp.Calculations;

public class RetirementV2
{
    public RetirementV2(RetirementV2Params param)
    {
        CalculationEventType = param.EventType;
        DateOfBirth = param.DateOfBirth;
        DatePensionableServiceCommenced = param.DatePensionableServiceCommenced;
        DateOfLeaving = param.DateOfLeaving;
        StatePensionDate = param.StatePensionDate;
        StatePensionDeduction = param.StatePensionDeduction;
        GMPAge = param.GMPAge;
        Post88GMPIncreaseCap = param.Post88GMPIncreaseCap;
        Pre88GMPAtGMPAge = param.Pre88GMPAtGMPAge;
        Post88GMPAtGMPAge = param.Post88GMPAtGMPAge;
        TransferInService = param.TransferInService;
        TotalPensionableService = param.TotalPensionableService;
        FinalPensionableSalary = param.FinalPensionableSalary;
        InternalAVCFundValue = param.InternalAVCFundValue;
        ExternalAvcFundValue = param.ExternalAvcFundValue;
        TotalAvcFundValue = param.TotalAvcFundValue;
        TotalFundValue = param.TotalFundValue;
        StandardLifetimeAllowance = param.StandardLifetimeAllowance;
        TotalLtaUsedPercentage = param.TotalLtaUsedPercentage;
        MinimumPermittedTotalLumpSum = param.MinimumPermittedTotalLumpSum;
        MaximumPermittedTotalLumpSum = param.MaximumPermittedTotalLumpSum;
        MaximumPermittedStandardLumpSum = param.MaximumPermittedStandardLumpSum;
        TotalLtaRemainingPercentage = param.TotalLtaRemainingPercentage;
        NormalMinimumPensionAge = param.NormalMinimumPensionAge;
        WordingFlags = param.WordingFlags;
        QuotesV2 = param.QuotesV2;
        InputEffectiveDate = param.InputEffectiveDate;
        ResidualFundValue = param.ResidualFundValue;
    }

    public string WordingFlagsAsString()
    {
        return string.Join(";", WordingFlags);
    }

    public string CalculationEventType { get; }
    public DateTime DateOfBirth { get; }
    public DateTime? DatePensionableServiceCommenced { get; }
    public DateTime? DateOfLeaving { get; }
    public DateTime? StatePensionDate { get; }
    public decimal? StatePensionDeduction { get; }
    public string GMPAge { get; }  //iso duration format

    public decimal? Post88GMPIncreaseCap { get; }
    public decimal? Pre88GMPAtGMPAge { get; }
    public decimal? Post88GMPAtGMPAge { get; }
    public string TransferInService { get; }  //iso duration format

    public string TotalPensionableService { get; }  //iso duration format
    public decimal? FinalPensionableSalary { get; }
    public decimal InternalAVCFundValue { get; }
    public decimal ExternalAvcFundValue { get; }

    public decimal TotalAvcFundValue { get; }
    public decimal? TotalFundValue { get; }
    public decimal StandardLifetimeAllowance { get; }
    public decimal TotalLtaUsedPercentage { get; }
    public decimal MinimumPermittedTotalLumpSum { get; }
    public decimal MaximumPermittedTotalLumpSum { get; }
    public decimal MaximumPermittedStandardLumpSum { get; }
    public decimal TotalLtaRemainingPercentage { get; }
    public string NormalMinimumPensionAge { get; } //iso duration format 
    public string InputEffectiveDate { get; }
    public decimal? ResidualFundValue { get; }

    public IEnumerable<QuoteV2> QuotesV2 { get; }
    public IEnumerable<string> WordingFlags { get; private set; }

    public decimal? TotalPension()
    {
        return
            QuotesV2.SingleOrDefault(x => x.Name == "fullPensionSurrender")?.Attributes.Single(x => x.Name == "totalPension").Value ??
            QuotesV2.SingleOrDefault(x => x.Name == "fullPensionDCAsUFPLS")?.Attributes.Single(x => x.Name == "totalPension").Value ??
            QuotesV2.SingleOrDefault(x => x.Name == "fullPension")?.Attributes.Single(x => x.Name == "totalPension").Value;
    }

    public bool IsAvc() => TotalAvcFundValue > 0 || ExternalAvcFundValue > 0;

    public decimal? TotalAVCFund()
    {
        return TotalAvcFundValue != 0 ? TotalAvcFundValue : null;
    }

    public bool HasAdditionalContributions()
    {
        return TotalAvcFundValue != 0;
    }

    public decimal? FullPensionYearlyIncome()
    {
        return
            QuotesV2.SingleOrDefault(x => x.Name == "fullPensionSurrender")?.Attributes.Single(x => x.Name == "totalPension").Value ??
            QuotesV2.SingleOrDefault(x => x.Name == "fullPensionDCAsUFPLS")?.Attributes.Single(x => x.Name == "totalPension").Value ??
            QuotesV2.SingleOrDefault(x => x.Name == "fullPension")?.Attributes.Single(x => x.Name == "totalPension").Value;
    }

    public decimal? MaxLumpSum()
    {
        return IsAvc() ?
            QuotesV2.SingleOrDefault(x => x.Name == "reducedPensionDCAsLumpSum")?.Attributes.Single(x => x.Name == "totalLumpSum").Value :
            QuotesV2.SingleOrDefault(x => x.Name == "reducedPension")?.Attributes.Single(x => x.Name == "totalLumpSum").Value ??
            QuotesV2.SingleOrDefault(x => x.Name == "reducedPensionSurrender")?.Attributes.Single(x => x.Name == "totalLumpSum").Value;
    }

    public decimal? MaxLumpSumYearlyIncome()
    {
        return IsAvc() ?
            QuotesV2.SingleOrDefault(x => x.Name == "reducedPensionDCAsLumpSum")?.Attributes.Single(x => x.Name == "totalPension").Value :
            QuotesV2.SingleOrDefault(x => x.Name == "reducedPension")?.Attributes.Single(x => x.Name == "totalPension").Value ??
            QuotesV2.SingleOrDefault(x => x.Name == "reducedPensionSurrender")?.Attributes.Single(x => x.Name == "totalPension").Value;
    }

    public QuoteV2 FindQuote(string name)
    {
        return QuotesV2.SingleOrDefault(x => x.Name == name);
    }

    public int? GMPAgeYears()
    {
        return GMPAge.ParseIsoDuration()?.Years;
    }

    public int? GMPAgeMonths()
    {
        return GMPAge.ParseIsoDuration()?.Months;
    }

    public int? NormalMinimumPensionAgeYears()
    {
        return NormalMinimumPensionAge.ParseIsoDuration()?.Years;
    }

    public int? NormalMinimumPensionAgeMonths()
    {
        return NormalMinimumPensionAge.ParseIsoDuration()?.Months;
    }

    public decimal? GetTotalLtaUsedPerc(string selectedQuoteName, string businessGroup, string schemeType)
    {
        if (schemeType == "DC")
            return TotalLtaUsedPercentage;

        if (selectedQuoteName == null)
            return null;

        var fullSelectedQuoteName = selectedQuoteName.Replace(".", "_");
        if (businessGroup.Equals("GSK", StringComparison.InvariantCultureIgnoreCase))
        {
            var rootQuote = fullSelectedQuoteName.Split("_").First();

            return QuotesV2.FirstOrDefault(x => x.Name == rootQuote)?.Attributes.FirstOrDefault(x => x.Name == "totalLTAUsedPerc")?.Value;
        }

        var selectedQuoteNameDetails = QuotesV2.FirstOrDefault(x => x.Name == fullSelectedQuoteName);
        return selectedQuoteNameDetails?.Attributes.FirstOrDefault(x => x.Name == "totalLTAUsedPerc")?.Value;
    }

    public void SetCalculationFailedWordingFlags(Error error)
    {
        if (IsCalculationFailed())
            return;

        var wordingFlags = WordingFlags.ToList();

        if (error.Inner.IsSome)
        {
            wordingFlags.Add(error.Inner.Value().ToString());
            WordingFlags = wordingFlags;
            return;
        }

        wordingFlags.Add("CalculationFailed");
        WordingFlags = wordingFlags;
    }

    public bool IsCalculationFailed()
    {
        return WordingFlags.Contains("CalculationFailed") || WordingFlags.Contains("noFigures");
    }
}