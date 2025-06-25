using System;
using System.Collections.Generic;

namespace WTW.MdpService.Domain.Mdp.Calculations;

public record RetirementDtoV2
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
    public decimal? TotalFundValue { get; set; }
    public decimal StandardLifetimeAllowance { get; set; }
    public decimal TotalLtaUsedPercentage { get; set; }
    public decimal MinimumPermittedTotalLumpSum { get; set; }
    public decimal MaximumPermittedTotalLumpSum { get; set; }
    public decimal MaximumPermittedStandardLumpSum { get; set; }
    public decimal TotalLtaRemainingPercentage { get; set; }
    public string NormalMinimumPensionAge { get; set; }
    public string InputEffectiveDate { get; set; }
    public IEnumerable<QuoteDtoV2> QuotesV2 { get; set; }
    public IEnumerable<string> WordingFlags { get; set; }
    public decimal? ResidualFundValue { get; set; }
}

public record QuoteDtoV2
{
    public string Name { get; set; }

    public IEnumerable<QuoteAttributesDtoV2> Attributes { get; set; }

    public IEnumerable<PensionTrancheDtoV2> PensionTranches { get; set; }
}

public record QuoteAttributesDtoV2
{
    public string Name { get; set; }
    public decimal? Value { get; set; }
}

public record PensionTrancheDtoV2
{
    public string TrancheTypeCode { get; set; }
    public string IncreaseTypeCode { get; set; }
    public decimal Value { get; set; }
}