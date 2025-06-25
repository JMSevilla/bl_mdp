using System;
using System.Linq;
using LanguageExt.Common;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.Web.Extensions;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Members;

public class MembershipSummary
{
    public MembershipSummary(Domain.Members.Member member, RetirementDatesAges retirementDatesAges, Domain.Mdp.Calculations.RetirementV2 retirement, DateTimeOffset utcNow)
    {
        bool isSpecialRetirementDate = retirementDatesAges.WordingFlags.Any(x => x == "SRD_USED");

        ReferenceNumber = member.ReferenceNumber;
        DateOfBirth = member.PersonalDetails.DateOfBirth;
        Title = member.PersonalDetails.Title;
        Forenames = member.PersonalDetails.Forenames;
        Surname = member.PersonalDetails.Surname;
        NormalRetirementAge = !string.IsNullOrEmpty(retirementDatesAges.TargetRetirementAgeIso) && !isSpecialRetirementDate ? retirementDatesAges.TargetRetirement() : retirementDatesAges.NormalRetirement();
        NormalRetirementDate = !isSpecialRetirementDate ? retirementDatesAges.TargetRetirementDate ?? retirementDatesAges.NormalRetirementDate : retirementDatesAges.NormalRetirementDate;
        DatePensionableServiceCommenced = retirement.DatePensionableServiceCommenced;
        DateOfLeaving = retirement.DateOfLeaving;
        TransferInServiceYears = retirement.TransferInService.ParseIsoDuration()?.Years;
        TransferInServiceMonths = retirement.TransferInService.ParseIsoDuration()?.Months;
        TotalPensionableServiceYears = retirement.TotalPensionableService.ParseIsoDuration()?.Years;
        TotalPensionableServiceMonths = retirement.TotalPensionableService.ParseIsoDuration()?.Months;
        FinalPensionableSalary = retirement.FinalPensionableSalary;
        InsuranceNumber = member.InsuranceNumber;
        MembershipNumber = member.MembershipNumber;
        Status = member.Status;
        PayrollNumber = member.PayrollNumber;
        DateJoinedScheme = member.DateJoinedScheme;
        DateLeftScheme = GetDateLeftScheme(member);
        SchemeName = member.Scheme.Name;
        FloorRoundedAge = Convert.ToInt32(member.GetExactAge(utcNow).HasValue ? Math.Floor(member.GetExactAge(utcNow).Value) : null);
        Age = Convert.ToInt32(member.GetExactAge(utcNow));
        HasAdditionalContributions = retirement.HasAdditionalContributions();
        Category = member.Category;
        DatePensionableServiceStarted = member.DatePensionableServiceStarted;
    }

    public MembershipSummary(Domain.Members.Member member, Result<RetirementDatesAgesResponse> retirementDatesAgesResponse, DateTimeOffset utcNow)
    {
        ReferenceNumber = member.ReferenceNumber;
        DateOfBirth = member.PersonalDetails.DateOfBirth;
        Title = member.PersonalDetails.Title;
        Forenames = member.PersonalDetails.Forenames;
        Surname = member.PersonalDetails.Surname;
        InsuranceNumber = member.InsuranceNumber;
        MembershipNumber = member.MembershipNumber;
        Status = member.Status;
        PayrollNumber = member.PayrollNumber;
        DateJoinedScheme = member.DateJoinedScheme;
        DateLeftScheme = GetDateLeftScheme(member);
        SchemeName = member.Scheme.Name;
        FloorRoundedAge = Convert.ToInt32(member.GetExactAge(utcNow).HasValue ? Math.Floor(member.GetExactAge(utcNow).Value) : null);
        Age = Convert.ToInt32(member.GetExactAge(utcNow));
        if (retirementDatesAgesResponse.IsSuccess)
        {
            bool isSpecialRetirementDate = retirementDatesAgesResponse.Value().WordingFlags.Any(x => x == "SRD_USED");
            var responseData = retirementDatesAgesResponse.Value();

            NormalRetirementAge = !string.IsNullOrEmpty(responseData.RetirementAges.TargetRetirementAgeIso) && !isSpecialRetirementDate ?
                decimal.ToInt32(responseData.RetirementAges.TargetRetirementAgeIso.ParseIsoDuration().Value.Years) :
                decimal.ToInt32(responseData.RetirementAges.NormalRetirementAge);

            NormalRetirementDate = !isSpecialRetirementDate
                ? responseData.RetirementDates.TargetRetirementDate ?? responseData.RetirementDates.NormalRetirementDate
                : responseData.RetirementDates.NormalRetirementDate;
        }
        else
        {
            NormalRetirementAge = default;
            NormalRetirementDate = default;
        }
        Category = member.Category;
        DatePensionableServiceStarted = member.DatePensionableServiceStarted;
    }

    public string ReferenceNumber { get; }
    public DateTimeOffset? DateOfBirth { get; }
    public string Title { get; }
    public string Forenames { get; }
    public string Surname { get; }
    public int NormalRetirementAge { get; }
    public DateTimeOffset NormalRetirementDate { get; }
    public DateTimeOffset? DatePensionableServiceCommenced { get; }
    public DateTimeOffset? DateOfLeaving { get; }
    public int? TransferInServiceYears { get; }
    public int? TransferInServiceMonths { get; }
    public int? TotalPensionableServiceYears { get; }
    public int? TotalPensionableServiceMonths { get; }
    public decimal? FinalPensionableSalary { get; }
    public string InsuranceNumber { get; }
    public string MembershipNumber { get; }
    public Domain.Members.MemberStatus Status { get; }
    public string PayrollNumber { get; }
    public DateTimeOffset DateJoinedScheme { get; }
    public DateTimeOffset? DateLeftScheme { get; }
    public string SchemeName { get; }
    public int? FloorRoundedAge { get; }
    public int? Age { get; }
    public bool HasAdditionalContributions { get; }
    public string Category { get; }
    public DateTimeOffset? DatePensionableServiceStarted { get; }

    private static DateTimeOffset? GetDateLeftScheme(Domain.Members.Member member)
    {
        return member.Status == Domain.Members.MemberStatus.Deferred ? member.DateLeftScheme : null;
    }

    public static string CacheKey(string businessGroup, string referenceNumber)
    {
        return $"membership-summary-{businessGroup}-{referenceNumber}";
    }
}