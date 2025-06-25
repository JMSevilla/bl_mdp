using System;

namespace WTW.MdpService.Members;

public record MembershipSummaryResponse
{
    public string ReferenceNumber { get; init; }
    public DateTimeOffset? DateOfBirth { get; init; }
    public string Title { get; init; }
    public string Forenames { get; init; }
    public string Surname { get; init; }
    public int NormalRetirementAge { get; init; }
    public DateTimeOffset NormalRetirementDate { get; init; }
    public DateTimeOffset? DatePensionableServiceCommenced { get; init; }
    public DateTimeOffset? DateOfLeaving { get; init; }
    public int? TransferInServiceYears { get; init; }
    public int? TransferInServiceMonths { get; init; }
    public int? TotalPensionableServiceYears { get; init; }
    public int? TotalPensionableServiceMonths { get; init; }
    public decimal? FinalPensionableSalary { get; init; }
    public string InsuranceNumber { get; init; }
    public Domain.Members.MemberStatus Status { get; init; }
    public string MembershipNumber { get; init; }
    public string PayrollNumber { get; init; }
    public DateTimeOffset DateJoinedScheme { get; init; }
    public DateTimeOffset? DateLeftScheme { get; init; }
    public string SchemeName { get; init; }
    public int? Age { get; init; }
    public int? FloorRoundedAge { get; init; }
    public bool HasAdditionalContributions { get; init; }
    public string Category { get; init; }
    public DateTimeOffset? DatePensionableServiceStarted { get; init;  }

    public static MembershipSummaryResponse From(MembershipSummary membershipSummary)
    {
        return new()
        {
            ReferenceNumber = membershipSummary.ReferenceNumber,
            DateOfBirth = membershipSummary.DateOfBirth,
            Title = membershipSummary.Title,
            Forenames = membershipSummary.Forenames,
            Surname = membershipSummary.Surname,
            NormalRetirementAge = membershipSummary.NormalRetirementAge,
            NormalRetirementDate = membershipSummary.NormalRetirementDate,
            DatePensionableServiceCommenced = membershipSummary.DatePensionableServiceCommenced,
            DateOfLeaving = membershipSummary.DateOfLeaving,
            TransferInServiceYears = membershipSummary.TransferInServiceYears,
            TransferInServiceMonths = membershipSummary.TransferInServiceMonths,
            TotalPensionableServiceYears = membershipSummary.TotalPensionableServiceYears,
            TotalPensionableServiceMonths = membershipSummary.TotalPensionableServiceMonths,
            FinalPensionableSalary = membershipSummary.FinalPensionableSalary,
            InsuranceNumber = membershipSummary.InsuranceNumber,
            MembershipNumber = membershipSummary.MembershipNumber,
            Status = membershipSummary.Status,
            PayrollNumber = membershipSummary.PayrollNumber,
            DateJoinedScheme = membershipSummary.DateJoinedScheme,
            DateLeftScheme = membershipSummary.DateLeftScheme,
            SchemeName = membershipSummary.SchemeName,
            Age = membershipSummary.Age,
            FloorRoundedAge = membershipSummary.FloorRoundedAge,
            HasAdditionalContributions = membershipSummary.HasAdditionalContributions,
            Category = membershipSummary.Category,
            DatePensionableServiceStarted = membershipSummary.DatePensionableServiceStarted,
        };
    }
}