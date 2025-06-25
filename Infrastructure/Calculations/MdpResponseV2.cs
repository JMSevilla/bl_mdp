using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Calculations;

public record MdpResponseV2
{
    [JsonPropertyName("options")]
    public JsonElement Options { get; set; }

    [JsonPropertyName("totalAVCFundValue")]
    public decimal TotalAvcFundValue { get; init; }

    [JsonPropertyName("totalFundValue")]
    public decimal? TotalFundValue { get; init; }

    [JsonPropertyName("standardLifetimeAllowance")]
    public decimal StandardLifetimeAllowance { get; init; }

    [JsonPropertyName("externalAVCFundValue")]
    public decimal ExternalAvcFundValue { get; init; }

    [JsonPropertyName("internalAVCFundValue")]
    public decimal InternalAVCFundValue { get; init; }

    [JsonPropertyName("totalLTARemainingPerc")]
    public decimal TotalLtaRemainingPercentage { get; init; }

    [JsonPropertyName("totalLTAUsedPerc")]
    public decimal TotalLtaUsedPercentage { get; init; }

    [JsonPropertyName("maximumPermittedTotalLumpSum")]
    public decimal MaximumPermittedTotalLumpSum { get; init; }

    [JsonPropertyName("minimumPermittedTotalLumpSum")]
    public decimal MinimumPermittedTotalLumpSum { get; init; }

    [JsonPropertyName("maximumPermittedStandardLumpSum")]
    public decimal MaximumPermittedStandardLumpSum { get; init; }

    [JsonPropertyName("datePensionableServiceCommenced")]
    public DateTime? DatePensionableServiceCommenced { get; init; }

    [JsonPropertyName("calculationFactorDate")]
    public DateTime CalculationFactorDate { get; init; }

    [JsonPropertyName("dateOfBirth")]
    public DateTime DateOfBirth { get; init; }

    [JsonPropertyName("dateOfLeaving")]
    public DateTime? DateOfLeaving { get; init; }

    [JsonPropertyName("statePensionDate")]
    public DateTime? StatePensionDate { get; init; }

    [JsonPropertyName("transferInService")]
    public string TransferInService { get; init; }

    [JsonPropertyName("totalPensionableService")]
    public string TotalPensionableService { get; init; }

    [JsonPropertyName("finalPensionableSalary")]
    public decimal? FinalPensionableSalary { get; init; }

    public List<string> WordingFlags { get; init; } = new();

    [JsonPropertyName("GMPAge")]
    public string GMPAge { get; set; }

    [JsonPropertyName("pre88GMPAtGMPAge")]
    public decimal? Pre88GMPAtGMPAge { get; set; }

    [JsonPropertyName("post88GMPAtGMPAge")]
    public decimal? Post88GMPAtGMPAge { get; set; }

    [JsonPropertyName("post88GMPIncreaseCap")]
    public decimal? Post88GMPIncreaseCap { get; set; }

    [JsonPropertyName("statePensionDeduction")]
    public decimal? StatePensionDeduction { get; set; }

    [JsonPropertyName("trancheIncreaseMethods")]
    public JsonElement? TrancheIncreaseMethods { get; set; }

    [JsonPropertyName("statutoryFactors")]
    public StatutoryFactors StatutoryFactors { get; set; }

    [JsonPropertyName("rateOfReturn")]
    public RateOfReturn RateOfReturn { get; set; }

    [JsonPropertyName("quotation")]
    public QuotationInfo Quotation { get; set; }

    [JsonPropertyName("calcSystemHistorySeqno")]
    public int CalcSystemHistorySeqno { get; set; } = 0;

    [JsonPropertyName("residualFundValue")]
    public decimal? ResidualFundValue { get; set; }
}

public class QuotationInfo
{
    [JsonPropertyName("guaranteed")]
    public bool Guaranteed { get; set; }

    [JsonPropertyName("expiryDate")]
    public DateTime? ExpiryDate { get; set; }
}
