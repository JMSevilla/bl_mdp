using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Calculations;

public record RetirementResponse
{
    public ErrorsResponse Errors { get; init; }
    public ResultsResponse Results { get; init; }

    public record ResultsResponse
    {
        public MdpResponse Mdp { get; init; }
    }

    public record MdpResponse
    {
        [JsonPropertyName("fullPensionDCAsTransfer")]
        public QuoteResponse FullPensionDCAsTransfer { get; init; }

        [JsonPropertyName("fullPensionDCAsUFPLS")]
        public QuoteResponse FullPensionDCAsUFPLS { get; init; }

        [JsonPropertyName("reducedPensionDCAsLumpSum")]
        public QuoteResponse ReducedPensionDCAsLumpSum { get; init; }

        [JsonPropertyName("reducedPensionDCAsTransfer")]
        public QuoteResponse ReducedPensionDCAsTransfer { get; init; }

        [JsonPropertyName("reducedPensionDCAsUFPLS")]
        public QuoteResponse ReducedPensionDCAsUFPLS { get; init; }

        [JsonPropertyName("fullPension")]
        public QuoteResponse FullPension { get; init; }

        [JsonPropertyName("reducedPension")]
        public QuoteResponse ReducedPension { get; init; }

        [JsonPropertyName("fullCommutation")]
        public QuoteResponse FullCommutation { get; init; }

        [JsonPropertyName("trivialCommutation")]
        public QuoteResponse TrivialCommutation { get; init; }

        [JsonPropertyName("smallPotCommutation")]
        public QuoteResponse SmallPotCommutation { get; init; }

        [JsonPropertyName("fullPensionDCAsOMOAnnuity")]
        public QuoteResponse FullPensionDCAsOMOAnnuity { get; init; }

        [JsonPropertyName("reducedPensionDCAsOMOAnnuity")]
        public QuoteResponse ReducedPensionDCAsOMOAnnuity { get; init; }

        [JsonPropertyName("totalAVCFundValue")]
        public decimal TotalAvcFundValue { get; init; }

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

        [JsonPropertyName("datePensionableServiceCommenced")]
        public DateTime? DatePensionableServiceCommenced { get; init; }

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

        public List<string> WordingFlags { get; init; }

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
        public JsonElement TrancheIncreaseMethods { get; set; }

        [JsonPropertyName("statutoryFactors")]
        public StatutoryFactors StatutoryFactors { get; set; }
    }

    public record StatutoryFactors
    {
        [JsonPropertyName("normalMinimumPensionAge")]
        public string NormalMinimumPensionAge { get; init; }

        [JsonPropertyName("standardLifetimeAllowance")]
        public decimal StandardLifetimeAllowance { get; init; }
    }

    public record QuoteResponse
    {
        [JsonPropertyName("lumpSumFromDB")]
        public decimal LumpSumFromDb { get; init; }

        [JsonPropertyName("lumpSumFromDC")]
        public decimal LumpSumFromDc { get; init; }

        [JsonPropertyName("smallPotLumpSum")]
        public decimal SmallPotLumpSum { get; init; }

        [JsonPropertyName("taxFreeUFPLS")]
        public decimal TaxFreeUfpls { get; init; }

        [JsonPropertyName("taxableUFPLS")]
        public decimal TaxableUfpls { get; init; }

        [JsonPropertyName("totalLumpSum")]
        public decimal TotalLumpSum { get; init; }

        [JsonPropertyName("totalPension")]
        public decimal TotalPension { get; init; }

        [JsonPropertyName("totalSpousePension")]
        public decimal TotalSpousePension { get; init; }

        [JsonPropertyName("totalUFPLS")]
        public decimal TotalUfpls { get; init; }

        [JsonPropertyName("transferValueOfDC")]
        public decimal TransferValueOfDc { get; init; }

        [JsonPropertyName("trivialCommutationLumpSum")]
        public decimal TrivialCommutationLumpSum { get; init; }

        [JsonPropertyName("annuityPurchaseAmount")]
        public decimal AnnuityPurchaseAmount { get; init; }

        [JsonPropertyName("pensionTranches")]
        public JsonElement PensionTranches { get; set; }

        [JsonPropertyName("minimumPermittedTotalLumpSum")]
        public decimal MinimumPermittedTotalLumpSum { get; init; }

        [JsonPropertyName("maximumPermittedTotalLumpSum")]
        public decimal MaximumPermittedTotalLumpSum { get; init; }
    }
}
