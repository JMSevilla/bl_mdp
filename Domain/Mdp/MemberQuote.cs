using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.Domain.Mdp;

public class MemberQuote : ValueObject
{
    protected MemberQuote() { }

    private MemberQuote(DateTimeOffset searchedRetirementDate,
        string label, decimal? annuityPurchaseAmount, decimal? lumpSumFromDb, decimal? lumpSumFromDc, decimal? smallPotLumpSum, decimal? taxFreeUfpls,
        decimal? taxableUfpls, decimal? totalLumpSum, decimal? totalPension, decimal? totalSpousePension,
        decimal? totalUfpls, decimal? transferValueOfDc, decimal? minimumLumpSum, decimal? maximumLumpSum,
        decimal? trivialCommutationLumpSum, bool hasAvcs, decimal ltaPercentage, int earliestRetirementAge,
        int normalRetirementAge, DateTimeOffset normalRetirementDate, DateTimeOffset? datePensionableServiceCommenced,
        DateTimeOffset? dateOfLeaving, string transferInService, string totalPensionableService, decimal? finalPensionableSalary,
        string calculationType, string wordingFlags, int pensionOptionNumber, decimal? statePensionDeduction)
    {
        SearchedRetirementDate = searchedRetirementDate;
        Label = label;
        AnnuityPurchaseAmount = annuityPurchaseAmount;
        LumpSumFromDb = lumpSumFromDb;
        LumpSumFromDc = lumpSumFromDc;
        SmallPotLumpSum = smallPotLumpSum;
        TaxFreeUfpls = taxFreeUfpls;
        TaxableUfpls = taxableUfpls;
        TotalLumpSum = totalLumpSum;
        TotalPension = totalPension;
        TotalSpousePension = totalSpousePension;
        TotalUfpls = totalUfpls;
        TransferValueOfDc = transferValueOfDc;
        MinimumLumpSum = minimumLumpSum;
        MaximumLumpSum = maximumLumpSum;
        TrivialCommutationLumpSum = trivialCommutationLumpSum;
        HasAvcs = hasAvcs;
        LtaPercentage = ltaPercentage;
        EarliestRetirementAge = earliestRetirementAge;
        NormalRetirementAge = normalRetirementAge;
        NormalRetirementDate = normalRetirementDate;
        DatePensionableServiceCommenced = datePensionableServiceCommenced;
        DateOfLeaving = dateOfLeaving;
        TransferInService = transferInService;
        TotalPensionableService = totalPensionableService;
        FinalPensionableSalary = finalPensionableSalary;
        CalculationType = calculationType;
        WordingFlags = wordingFlags;
        PensionOptionNumber = pensionOptionNumber;
        StatePensionDeduction = statePensionDeduction;
    }

    private MemberQuote(DateTimeOffset searchedRetirementDate,
        string label, bool hasAvcs, decimal ltaPercentage, int earliestRetirementAge,
        int normalRetirementAge, DateTimeOffset normalRetirementDate, DateTimeOffset? datePensionableServiceCommenced,
        DateTimeOffset? dateOfLeaving, string transferInService, string totalPensionableService, decimal? finalPensionableSalary,
        string calculationType, string wordingFlags, decimal? statePensionDeduction)
    {
        SearchedRetirementDate = searchedRetirementDate;
        Label = label;
        HasAvcs = hasAvcs;
        LtaPercentage = ltaPercentage;
        EarliestRetirementAge = earliestRetirementAge;
        NormalRetirementAge = normalRetirementAge;
        NormalRetirementDate = normalRetirementDate;
        DatePensionableServiceCommenced = datePensionableServiceCommenced;
        DateOfLeaving = dateOfLeaving;
        TransferInService = transferInService;
        TotalPensionableService = totalPensionableService;
        FinalPensionableSalary = finalPensionableSalary;
        CalculationType = calculationType;
        WordingFlags = wordingFlags;
        StatePensionDeduction = statePensionDeduction;
    }

    public DateTimeOffset SearchedRetirementDate { get; }
    public string Label { get; }
    public decimal? AnnuityPurchaseAmount { get; }
    public decimal? LumpSumFromDb { get; }
    public decimal? LumpSumFromDc { get; }
    public decimal? SmallPotLumpSum { get; }
    public decimal? TaxFreeUfpls { get; }
    public decimal? TaxableUfpls { get; }
    public decimal? TotalLumpSum { get; }
    public decimal? TotalPension { get; }
    public decimal? TotalSpousePension { get; }
    public decimal? TotalUfpls { get; }
    public decimal? TransferValueOfDc { get; }
    public decimal? MinimumLumpSum { get; }
    public decimal? MaximumLumpSum { get; }
    public decimal? TrivialCommutationLumpSum { get; }
    public bool HasAvcs { get; }
    public decimal LtaPercentage { get; }
    public int EarliestRetirementAge { get; }
    public int NormalRetirementAge { get; }
    public DateTimeOffset NormalRetirementDate { get; }
    public DateTimeOffset? DatePensionableServiceCommenced { get; }
    public DateTimeOffset? DateOfLeaving { get; }
    public string TransferInService { get; }
    public string TotalPensionableService { get; }
    public decimal? FinalPensionableSalary { get; }
    public string CalculationType { get; }
    public int PensionOptionNumber { get; }
    public string WordingFlags { get; }
    public decimal? StatePensionDeduction { get; }

    public static Either<Error, MemberQuote> Create(DateTimeOffset searchedRetirementDate,
        string label, decimal? annuityPurchaseAmount, decimal? lumpSumFromDb, decimal? lumpSumFromDc, decimal? smallPotLumpSum, decimal? taxFreeUfpls,
        decimal? taxableUfpls, decimal? totalLumpSum, decimal? totalPension, decimal? totalSpousePension,
        decimal? totalUfpls, decimal? transferValueOfDc, decimal? minimumLumpSum, decimal? maximumLumpSum,
        decimal? trivialCommutationLumpSum, bool hasAvcs, decimal ltaPercentage, int earliestRetirementAge, int normalRetirementAge,
        DateTimeOffset normalRetirementDate, DateTimeOffset? datePensionableServiceCommenced, DateTimeOffset? dateOfLeaving,
        string transferInService, string totalPensionableService, decimal? finalPensionableSalary, string calculationType,
        string wordingFlags, int pensionOptionNumber, decimal? statePensionDeduction)
    {
        if (searchedRetirementDate == default)
            return Error.New("Date should have non-default value.");

        if (string.IsNullOrWhiteSpace(label))
            return Error.New("Label should have a value.");

        return new MemberQuote(searchedRetirementDate, label, annuityPurchaseAmount, lumpSumFromDb, lumpSumFromDc, smallPotLumpSum, taxFreeUfpls,
            taxableUfpls, totalLumpSum, totalPension, totalSpousePension, totalUfpls, transferValueOfDc, minimumLumpSum,
            maximumLumpSum, trivialCommutationLumpSum, hasAvcs, ltaPercentage, earliestRetirementAge, normalRetirementAge,
            normalRetirementDate, datePensionableServiceCommenced, dateOfLeaving, transferInService,
            totalPensionableService, finalPensionableSalary, calculationType, wordingFlags, pensionOptionNumber, statePensionDeduction);
    }

    public static Either<Error, MemberQuote> CreateV2(DateTimeOffset searchedRetirementDate,
       string label, bool hasAvcs, decimal ltaPercentage, int earliestRetirementAge, int normalRetirementAge,
       DateTimeOffset normalRetirementDate, DateTimeOffset? datePensionableServiceCommenced, DateTimeOffset? dateOfLeaving,
       string transferInService, string totalPensionableService, decimal? finalPensionableSalary, string calculationType,
       string wordingFlags, decimal? statePensionDeduction)
    {
        if (searchedRetirementDate == default)
            return Error.New("Date should have non-default value.");

        if (string.IsNullOrWhiteSpace(label))
            return Error.New("Label should have a value.");

        return new MemberQuote(searchedRetirementDate, label, hasAvcs, ltaPercentage, earliestRetirementAge, normalRetirementAge,
            normalRetirementDate, datePensionableServiceCommenced, dateOfLeaving, transferInService,
            totalPensionableService, finalPensionableSalary, calculationType, wordingFlags, statePensionDeduction);
    }

    public IEnumerable<string> ParsedWordingFlags()
    {
        return WordingFlags.Split(";").Where(x => !string.IsNullOrWhiteSpace(x));
    }

    protected override IEnumerable<object> Parts()
    {
        yield return SearchedRetirementDate;
        yield return Label;
        yield return AnnuityPurchaseAmount;
        yield return LumpSumFromDb;
        yield return LumpSumFromDc;
        yield return SmallPotLumpSum;
        yield return TaxFreeUfpls;
        yield return TaxableUfpls;
        yield return TotalLumpSum;
        yield return TotalPension;
        yield return TotalSpousePension;
        yield return TotalUfpls;
        yield return TransferValueOfDc;
        yield return MinimumLumpSum;
        yield return MaximumLumpSum;
        yield return TrivialCommutationLumpSum;
        yield return HasAvcs;
        yield return LtaPercentage;
        yield return EarliestRetirementAge;
        yield return NormalRetirementAge;
        yield return NormalRetirementDate;
        yield return DatePensionableServiceCommenced;
        yield return DateOfLeaving;
        yield return TransferInService;
        yield return TotalPensionableService;
        yield return FinalPensionableSalary;
        yield return CalculationType;
        yield return PensionOptionNumber;
        yield return WordingFlags;
        yield return StatePensionDeduction;
    }
}