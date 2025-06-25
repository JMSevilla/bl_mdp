using System;
using WTW.MdpService.Domain.Common;

namespace WTW.MdpService.Retirement;

public record CmsTokenInformationResponse
{
    public bool IsCalculationSuccessful { get; set; }
    public int NormalRetirementAge { get; set; }
    public int EarliestRetirementAge { get; set; }
    public DateTimeOffset? EarliestRetirementDate { get; set; }
    public DateTimeOffset? SelectedRetirementDate { get; set; }
    public int? SelectedRetirementAge { get; set; }
    public int? LatestRetirementAge { get; set; }
    public decimal? ChosenLtaPercentage { get; set; }
    public decimal RemainingLtaPercentage { get; set; }
    public decimal LtaLimit { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public DateTimeOffset? RetirementJourneyExpirationDate { get; set; }
    public DateTimeOffset? NormalRetirementDate { get; set; }
    public DateTimeOffset? TargetRetirementDate { get; set; }
    public string TargetRetirementAgeIso { get; set; }
    public int? TargetRetirementAge { get; set; }
    public string TimeToTargetRetirementIso { get; set; }
    public int? GmpAgeYears { get; set; }
    public int? GmpAgeMonths { get; set; }
    public decimal? Pre88GMPAtGMPAge { get; set; }
    public decimal? Post88GMPAtGMPAge { get; set; }
    public decimal? Post88GMPIncreaseCap { get; set; }
    public decimal? StatePensionDeduction { get; set; }
    public DateTime SystemDate { get; set; }
    public int? NormalMinimumPensionAgeYears { get; set; }
    public int? NormalMinimumPensionAgeMonths { get; set; }
    public string InsuranceNumber { get; set; }
    public Address Address { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public DateTimeOffset? TransferReplyByDate { get; set; }
    public DateTimeOffset? TransferGuaranteeExpiryDate { get; set; }
    public int? TransferGuaranteePeriodMonths { get; set; }
    public DateTimeOffset? TransferQuoteRunDate { get; set; }
    public string PensionPaymentDay { get; set; }
    public string Name { get; set; }
    public string TimeToNormalRetirementIso { get; set; }
    public string CurrentAgeIso { get; set; }
    public decimal? TotalPension { get; set; }
    public decimal? TotalAVCFund { get; set; }
    public string AgeAtNormalRetirementIso { get; set; }
    public DateTimeOffset? MemberNormalRetirementDate { get => NormalRetirementDate; }
    public DateTimeOffset? SubmissionDate { get; set; }
    public DateTimeOffset? QuoteExpiryDate { get; set; }
    public string AgeAtSelectedRetirementDateIso { get; set; }    
}