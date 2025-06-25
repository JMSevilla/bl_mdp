#nullable enable

namespace WTW.MdpService.Infrastructure.MemberService;

public class GetPensionDetailsClientResponse
{
    public string? ActiveFraud { get; set; }
    public string? MembershipNo { get; set; }
    public string? PayrollNo { get; set; }
    public string? DateJoinedCompany { get; set; }
    public string? DateJoinedScheme { get; set; }
    public string? DateCOCommenced { get; set; }
    public string? DateCOEnded { get; set; }
    public string? DateQualifyingServiceCommenced { get; set; }
    public string? SpecialRetirementDate { get; set; }
    public string? TargetRetirementDate { get; set; }
    public string? PensionSharingStatus { get; set; }
    public string? TaxOfficeReference { get; set; }
    public string? TaxOfficeEmployerReference { get; set; }
    public string? PayCode { get; set; }
    public string? ClientReferenceNumber { get; set; }
    public string? ContractedOutReferenceNumber { get; set; }
    public string? ExtendedPayrollNumber { get; set; }
    public string? DCPaypoint { get; set; }
    public string? ReducedRateIndicator { get; set; }
    public string? DateReducedRaterevoked { get; set; }
    public string? MemberSecurityLevel { get; set; }
    public long? NIEarningsToDateRevoked { get; set; }
    public string? ElectionofIRBasis { get; set; }
    public decimal? EPBPerAnnum { get; set; }
    public decimal? ContributionRate { get; set; }
    public decimal? PaymentInLieu { get; set; }
    public decimal? LAMultiple { get; set; }
    public string? RecalculateExitBenefit { get; set; }
    public decimal? LAFixedAmount { get; set; }
    public string? NonStandardCommsIndicator { get; set; }
    public int? MinimumPensionAge { get; set; }
    public string? InvestContributions { get; set; }
    public string? AANominatedDate { get; set; }
    public decimal? LTAPercent { get; set; }
    public string? EligWaitPeriodORDate { get; set; }
    public string? ExternalPensionSchemeCode { get; set; }
    public string? ContractualEnrolment { get; set; }
    public string? PensionsEligibilityFlag { get; set; }
    public string? FlexRetirementMethod { get; set; }
}
#nullable disable