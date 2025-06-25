using System;
using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.Calculations;

public record RetirementDto
{
    public string CalculationEventType { get; set; }
    public DateTimeOffset EffectiveDate { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime? DatePensionableServiceCommenced { get; set; }
    public DateTime? DateOfLeaving { get; set; }
    public DateTime? StatePensionDate { get; set; }
    public decimal? StatePensionDeduction { get; set; }
    public string GMPAge { get; set; }

    public decimal? Post88GMPIncreaseCap { get; set; }
    public decimal? Pre88GMPAtGMPAge { get; set; }
    public decimal? Post88GMPAtGMPAge { get; set; }
    public string TransferInService { get; set; }
    public string TotalPensionableService { get; set; }
    public decimal? FinalPensionableSalary { get; set; }
    public decimal InternalAVCFundValue { get; set; }
    public decimal ExternalAvcFundValue { get; set; }

    public decimal TotalAvcFundValue { get; set; }
    public decimal StandardLifetimeAllowance { get; set; }
    public decimal TotalLtaUsedPercentage { get; set; }
    public decimal MaximumPermittedTotalLumpSum { get; set; }
    public decimal TotalLtaRemainingPercentage { get; set; }
    public string NormalMinimumPensionAge { get; set; }
    public IEnumerable<QuoteDto> Quotes { get; set; }
    public IEnumerable<string> WordingFlags { get; set; }
}

public record QuoteDto
{
    public string Name { get; set; }
    public int SequenceNumber { get; set; }
    public decimal? LumpSumFromDb { get; set; }
    public decimal? LumpSumFromDc { get; set; }
    public decimal? SmallPotLumpSum { get; set; }
    public decimal? TaxFreeUfpls { get; set; }
    public decimal? TaxableUfpls { get; set; }
    public decimal? TotalLumpSum { get; set; }
    public decimal? TotalPension { get; set; }
    public decimal? TotalSpousePension { get; set; }
    public decimal? TotalUfpls { get; set; }
    public decimal? TransferValueOfDc { get; set; }
    public decimal? TrivialCommutationLumpSum { get; set; }
    public decimal? AnnuityPurchaseAmount { get; set; }
    public decimal? MinimumLumpSum { get; set; }
    public decimal? MaximumLumpSum { get; set; }
    public IEnumerable<PensionTrancheDto> PensionTranches { get; set; }
}

public record PensionTrancheDto
{
    public string TrancheTypeCode { get; set; }
    public string IncreaseTypeCode { get; set; }
    public decimal Value { get; set; }
}