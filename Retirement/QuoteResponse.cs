using System.Collections.Generic;
using System.Linq;

namespace WTW.MdpService.Retirement;

public record QuoteResponse
{
    public string Label { get; init; }
    public int SequenceNumber { get; init; }
    public decimal? LumpSumFromDb { get; init; }
    public decimal? LumpSumFromDc { get; init; }
    public decimal? SmallPotLumpSum { get; init; }
    public decimal? TaxFreeUfpls { get; init; }
    public decimal? TaxableUfpls { get; init; }
    public decimal? TotalLumpSum { get; init; }
    public decimal? TotalPension { get; init; }
    public decimal? TotalSpousePension { get; init; }
    public decimal? TotalUfpls { get; init; }
    public decimal? TransferValueOfDc { get; init; }
    public decimal? MinimumLumpSum { get; init; }
    public decimal? MaximumLumpSum { get; init; }
    public decimal? TrivialCommutationLumpSum { get; init; }
    public decimal? AnnuityPurchaseAmount { get; init; }
    public List<PensionTrancheResponse> PensionTranches { get; init; }

    public static QuoteResponse From(Domain.Mdp.Calculations.Quote quote)
    {
        return new QuoteResponse
        {
            Label = quote.Name,
            SequenceNumber = quote.SequenceNumber,
            LumpSumFromDb = quote.LumpSumFromDb,
            LumpSumFromDc = quote.LumpSumFromDc,
            SmallPotLumpSum = quote.SmallPotLumpSum,
            TaxFreeUfpls = quote.TaxFreeUfpls,
            TaxableUfpls = quote.TaxableUfpls,
            TotalLumpSum = quote.TotalLumpSum,
            TotalPension = quote.TotalPension,
            TotalSpousePension = quote.TotalSpousePension,
            TotalUfpls = quote.TotalUfpls,
            TransferValueOfDc = quote.TransferValueOfDc,
            TrivialCommutationLumpSum = quote.TrivialCommutationLumpSum,
            AnnuityPurchaseAmount = quote.AnnuityPurchaseAmount,
            MaximumLumpSum = quote.MaximumLumpSum,
            MinimumLumpSum = quote.MinimumLumpSum,
            PensionTranches = quote.PensionTranches.Select(PensionTrancheResponse.From).ToList()
        };
    }
}

public record PensionTrancheResponse
{
    public string TrancheTypeCode { get; init; }
    public string IncreaseTypeCode { get; init; }
    public decimal Value { get; init; }

    public static PensionTrancheResponse From(Domain.Mdp.Calculations.PensionTranche pensionTranch)
    {
        return new()
        {
            TrancheTypeCode = pensionTranch.TrancheTypeCode,
            IncreaseTypeCode = pensionTranch.IncreaseTypeCode,
            Value = pensionTranch.Value,
        };
    }
}