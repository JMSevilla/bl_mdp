using System.Collections.Generic;

namespace WTW.MdpService.Domain.Mdp.Calculations;

public class Quote
{
    public Quote(string name,
        int sequenceNumber,
        decimal? lumpSumFromDb,
        decimal? lumpSumFromDc,
        decimal? smallPotLumpSum,
        decimal? taxFreeUfpls,
        decimal? taxableUfpls,
        decimal? totalLumpSum,
        decimal? totalPension,
        decimal? totalSpousePension,
        decimal? totalUfpls,
        decimal? transferValueOfDc,
        decimal? trivialCommutationLumpSum,
        decimal? annuityPurchaseAmount,
        decimal? minimumLumpSum,
        decimal? maximumLumpSum,
        IEnumerable<PensionTranche> pensionTranches)
    {
        Name = name;
        SequenceNumber = sequenceNumber;
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
        TrivialCommutationLumpSum = trivialCommutationLumpSum;
        AnnuityPurchaseAmount = annuityPurchaseAmount;
        MinimumLumpSum = minimumLumpSum;
        MaximumLumpSum = maximumLumpSum;
        PensionTranches = pensionTranches;
    }

    public string Name { get; }
    public int SequenceNumber { get; }
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
    public decimal? TrivialCommutationLumpSum { get; }
    public decimal? AnnuityPurchaseAmount { get; }
    public decimal? MinimumLumpSum { get; }
    public decimal? MaximumLumpSum { get; }
    public IEnumerable<PensionTranche> PensionTranches { get; }
}