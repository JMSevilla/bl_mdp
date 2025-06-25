using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.Web.Extensions;

namespace WTW.MdpService.Domain.Mdp.Calculations;

public class Retirement
{
    public Retirement(RetirementResponse retirementResponse, string eventType)
    {
        CalculationEventType = eventType;

        DateOfBirth = retirementResponse.Results.Mdp.DateOfBirth;
        DatePensionableServiceCommenced = retirementResponse.Results.Mdp.DatePensionableServiceCommenced;
        DateOfLeaving = retirementResponse.Results.Mdp.DateOfLeaving;
        StatePensionDate = retirementResponse.Results.Mdp.StatePensionDate;
        StatePensionDeduction = retirementResponse.Results.Mdp.StatePensionDeduction;
        GMPAge = retirementResponse.Results.Mdp.GMPAge;

        Post88GMPIncreaseCap = retirementResponse.Results.Mdp.Post88GMPIncreaseCap;
        Pre88GMPAtGMPAge = retirementResponse.Results.Mdp.Pre88GMPAtGMPAge;
        Post88GMPAtGMPAge = retirementResponse.Results.Mdp.Post88GMPAtGMPAge;
        TransferInService = retirementResponse.Results.Mdp.TransferInService;

        TotalPensionableService = retirementResponse.Results.Mdp.TotalPensionableService;
        FinalPensionableSalary = retirementResponse.Results.Mdp.FinalPensionableSalary;
        InternalAVCFundValue = retirementResponse.Results.Mdp.InternalAVCFundValue;
        ExternalAvcFundValue = retirementResponse.Results.Mdp.ExternalAvcFundValue;

        TotalAvcFundValue = retirementResponse.Results.Mdp.TotalAvcFundValue;
        StandardLifetimeAllowance = retirementResponse.Results.Mdp.StandardLifetimeAllowance;
        TotalLtaUsedPercentage = retirementResponse.Results.Mdp.TotalLtaUsedPercentage;
        MaximumPermittedTotalLumpSum = retirementResponse.Results.Mdp.MaximumPermittedTotalLumpSum;
        TotalLtaRemainingPercentage = retirementResponse.Results.Mdp.TotalLtaRemainingPercentage;
        NormalMinimumPensionAge = retirementResponse.Results.Mdp.StatutoryFactors?.NormalMinimumPensionAge;

        WordingFlags = retirementResponse.Results.Mdp.WordingFlags.Concat(TrancheIncreaseMethodsWordingFlags(retirementResponse.Results.Mdp.TrancheIncreaseMethods));
        Quotes = ParseQuotes(retirementResponse.Results.Mdp);
    }

    public Retirement(RetirementDto dto)
    {
        CalculationEventType = dto.CalculationEventType;
        DateOfBirth = dto.DateOfBirth;
        DatePensionableServiceCommenced = dto.DatePensionableServiceCommenced;
        DateOfLeaving = dto.DateOfLeaving;
        StatePensionDate = dto.StatePensionDate;
        StatePensionDeduction = dto.StatePensionDeduction;
        GMPAge = dto.GMPAge;
        Post88GMPIncreaseCap = dto.Post88GMPIncreaseCap;
        Pre88GMPAtGMPAge = dto.Pre88GMPAtGMPAge;
        Post88GMPAtGMPAge = dto.Post88GMPAtGMPAge;
        TransferInService = dto.TransferInService;
        TotalPensionableService = dto.TotalPensionableService;
        FinalPensionableSalary = dto.FinalPensionableSalary;
        InternalAVCFundValue = dto.InternalAVCFundValue;
        ExternalAvcFundValue = dto.ExternalAvcFundValue;
        TotalAvcFundValue = dto.TotalAvcFundValue;
        StandardLifetimeAllowance = dto.StandardLifetimeAllowance;
        TotalLtaUsedPercentage = dto.TotalLtaUsedPercentage;
        MaximumPermittedTotalLumpSum = dto.MaximumPermittedTotalLumpSum;
        TotalLtaRemainingPercentage = dto.TotalLtaRemainingPercentage;
        NormalMinimumPensionAge = dto.NormalMinimumPensionAge;
        DateOfLeaving = dto.DateOfLeaving;
        Quotes = dto.Quotes.Select(x =>
            new Quote(
                x.Name,
                x.SequenceNumber,
                x.LumpSumFromDb,
                x.LumpSumFromDc,
                x.SmallPotLumpSum,
                x.TaxFreeUfpls,
                x.TaxableUfpls,
                x.TotalLumpSum,
                x.TotalPension,
                x.TotalSpousePension,
                x.TotalUfpls,
                x.TransferValueOfDc,
                x.TrivialCommutationLumpSum,
                x.AnnuityPurchaseAmount,
                x.MinimumLumpSum,
                x.MaximumLumpSum,
                x.PensionTranches.Select(y => new PensionTranche(y.TrancheTypeCode, y.Value, y.IncreaseTypeCode))));
        WordingFlags = dto.WordingFlags;
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
    public decimal StandardLifetimeAllowance { get; }
    public decimal TotalLtaUsedPercentage { get; }   
    public decimal MaximumPermittedTotalLumpSum { get; }
    public decimal TotalLtaRemainingPercentage { get; }  
    public string NormalMinimumPensionAge { get; } //iso duration format 

    public IEnumerable<Quote> Quotes { get; }
    public IEnumerable<string> WordingFlags { get; private set; }

    public void UpdateWordingFlags(IEnumerable<string> wordingFlags)
    {
        WordingFlags = wordingFlags;
    }

    public string WordingFlagsAsString()
    {
        return string.Join(";", WordingFlags);
    }

    public decimal? TotalPension()
    {
        return
            Quotes.SingleOrDefault(x => x.Name == "FullPensionDCAsUFPLS")?.TotalPension ??
            Quotes.SingleOrDefault(x => x.Name == "FullPension")?.TotalPension;
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
            Quotes.SingleOrDefault(x => x.Name == "FullPensionDCAsUFPLS")?.TotalPension ??
            Quotes.SingleOrDefault(x => x.Name == "FullPension")?.TotalPension;
    }

    public decimal? MaxLumpSum()
    {
        return IsAvc() ?
            Quotes.SingleOrDefault(x => x.Name == "ReducedPensionDCAsLumpSum")?.TotalLumpSum :
            Quotes.SingleOrDefault(x => x.Name == "ReducedPension")?.TotalLumpSum;
    }

    public decimal? MaxLumpSumYearlyIncome()
    {
        return IsAvc() ?
            Quotes.SingleOrDefault(x => x.Name == "ReducedPensionDCAsLumpSum")?.TotalPension :
            Quotes.SingleOrDefault(x => x.Name == "ReducedPension")?.TotalPension;
    }

    public Quote FindQuote(string name)
    {
        return Quotes.Single(x => x.Name == name);
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

    private IReadOnlyList<Quote> ParseQuotes(RetirementResponse.MdpResponse mdp)
    {
        var quotes = new List<Quote>();

        quotes.Add(CreateQuote(mdp.FullPensionDCAsTransfer, nameof(mdp.FullPensionDCAsTransfer), mdp.TrancheIncreaseMethods, SequenceNumber(quotes)));
        quotes.Add(CreateQuote(mdp.FullPensionDCAsUFPLS, nameof(mdp.FullPensionDCAsUFPLS), mdp.TrancheIncreaseMethods, SequenceNumber(quotes)));
        quotes.Add(CreateQuote(mdp.ReducedPensionDCAsLumpSum, nameof(mdp.ReducedPensionDCAsLumpSum), mdp.TrancheIncreaseMethods, SequenceNumber(quotes)));
        quotes.Add(CreateQuote(mdp.ReducedPensionDCAsTransfer, nameof(mdp.ReducedPensionDCAsTransfer), mdp.TrancheIncreaseMethods, SequenceNumber(quotes)));
        quotes.Add(CreateQuote(mdp.ReducedPensionDCAsUFPLS, nameof(mdp.ReducedPensionDCAsUFPLS), mdp.TrancheIncreaseMethods, SequenceNumber(quotes)));
        quotes.Add(CreateQuote(mdp.FullPension, nameof(mdp.FullPension), mdp.TrancheIncreaseMethods, SequenceNumber(quotes)));
        quotes.Add(CreateQuote(mdp.ReducedPension, nameof(mdp.ReducedPension), mdp.TrancheIncreaseMethods, SequenceNumber(quotes)));
        quotes.Add(CreateQuote(mdp.FullCommutation, nameof(mdp.FullCommutation), mdp.TrancheIncreaseMethods, SequenceNumber(quotes)));
        quotes.Add(CreateQuote(mdp.TrivialCommutation, nameof(mdp.TrivialCommutation), mdp.TrancheIncreaseMethods, SequenceNumber(quotes)));
        quotes.Add(CreateQuote(mdp.SmallPotCommutation, nameof(mdp.SmallPotCommutation), mdp.TrancheIncreaseMethods, SequenceNumber(quotes)));
        quotes.Add(CreateQuote(mdp.FullPensionDCAsOMOAnnuity, nameof(mdp.FullPensionDCAsOMOAnnuity), mdp.TrancheIncreaseMethods, SequenceNumber(quotes)));
        quotes.Add(CreateQuote(mdp.ReducedPensionDCAsOMOAnnuity, nameof(mdp.ReducedPensionDCAsOMOAnnuity), mdp.TrancheIncreaseMethods, SequenceNumber(quotes)));

        return quotes.Where(x => x != null).ToList();

        static int SequenceNumber(List<Quote> quotes) => quotes.Count(x => x != null) + 1;
    }

    private static Quote CreateQuote(
        RetirementResponse.QuoteResponse quote,
        string name,
        JsonElement trancheIncreaseMethods,
        int sequenceNumber)
    {
        if (quote == null)
            return null;

        return new Quote(
            name,
            sequenceNumber,
            quote.LumpSumFromDb == 0 ? null : quote.LumpSumFromDb,
            quote.LumpSumFromDc == 0 ? null : quote.LumpSumFromDc,
            quote.SmallPotLumpSum == 0 ? null : quote.SmallPotLumpSum,
            quote.TaxFreeUfpls == 0 ? null : quote.TaxFreeUfpls,
            quote.TaxableUfpls == 0 ? null : quote.TaxableUfpls,
            quote.TotalLumpSum == 0 ? null : quote.TotalLumpSum,
            quote.TotalPension == 0 ? null : quote.TotalPension,
            quote.TotalSpousePension,
            quote.TotalUfpls == 0 ? null : quote.TotalUfpls,
            quote.TransferValueOfDc == 0 ? null : quote.TransferValueOfDc,
            quote.TrivialCommutationLumpSum == 0 ? null : quote.TrivialCommutationLumpSum,
            quote.AnnuityPurchaseAmount == 0 ? null : quote.AnnuityPurchaseAmount,
            quote.MinimumPermittedTotalLumpSum == 0 ? null : quote.MinimumPermittedTotalLumpSum,
            quote.MaximumPermittedTotalLumpSum == 0 ? null : quote.MaximumPermittedTotalLumpSum,
            ParsePensionTranches(quote.PensionTranches, trancheIncreaseMethods));
    }

    private static IEnumerable<PensionTranche> ParsePensionTranches(JsonElement pensionTranches, JsonElement trancheIncreaseMethods)
    {
        return pensionTranches.EnumerateObject()
            .Where(x => !x.NameEquals("total"))
            .Select(x => new PensionTranche(
                x.Name,
                decimal.Parse(x.Value.ToString()),
                trancheIncreaseMethods.EnumerateObject().FirstOrDefault(y => y.NameEquals(x.Name)).Value.ToString()));
    }

    private static List<string> TrancheIncreaseMethodsWordingFlags(JsonElement trancheIncreaseMethods)
    {
        return trancheIncreaseMethods.EnumerateObject().Select(x => "tranche_" + x.Name + "_" + x.Value.ToString()).ToList();
    }
}