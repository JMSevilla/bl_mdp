using System;
using System.Collections.Generic;

namespace WTW.MdpService.Domain.Mdp.Calculations;

public class RetirementV2Params
{
    public string EventType { get; set; }
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
    public IEnumerable<QuoteV2> QuotesV2 { get; set; }
    public IEnumerable<string> WordingFlags { get; set; }
    public decimal? ResidualFundValue { get; set; }
}