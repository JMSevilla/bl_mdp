#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using WTW.MdpService.Content.V2;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.Web;
using WTW.Web.Extensions;

namespace WTW.MdpService.Members;

public record MemberAnalyticsResponse
{   
    public string BusinessGroup { get; init; }
    public MemberStatus Status { get; init; }
    public string SchemeType { get; init; }
    public string SchemeCode { get; init; }
    public string CategoryCode { get; init; }
    public string LocationCode { get; init; }
    public string EmployerCode { get; init; }
    public string Gender { get; set; }
    public string MaritalStatus { get; set; }
    public int? Tenure { get; set; }
    public bool IsAvc { get; set; }
    public PensionIncome Income { get; set; }
    public int? NormalRetirementAge { get; set; }
    public int? EarliestRetirementAge { get; set; }
    public int? LatestRetirementAge { get; set; }
    public int? TargetRetirementAge { get; set; }
    public DateTimeOffset? NormalRetirementDate { get; set; }
    public DateTimeOffset? TargetRetirementDate { get; set; }
    public MemberLifeStage? LifeStage { get; set; }
    public RetirementApplicationStatus? RetirementApplicationStatus { get; set; }
    public TransferApplicationStatus? TransferApplicationStatus { get; set; }
    public string? TenantUrl { get; set; }
    public bool? HasAdditionalContributions { get; set; }
    public string? DbCalculationStatus { get; set; }
    public string? UserId { get; set; }
    public string? DcRetirementJourney { get; set; }
    public bool DcexploreoptionsStarted { get; set; }
    public bool DcretirementapplicationStarted { get; set; }
    public bool DcretirementapplicationSubmitted { get; set; }
    public string? CurrentAge { get; set; }
    public string? DcLifeStage { get; set; }

    public static MemberAnalyticsResponse From(Member member, bool isAvc, RetirementDatesAges? retirementDatesAges, string userId, AccessKey? accessKey, string dcJourneyStatus)
    {
        return new()
        {
            BusinessGroup = member.BusinessGroup,
            Status = member.Status,
            SchemeType = member.Scheme.Type,
            SchemeCode = member.SchemeCode,
            CategoryCode = member.Category,
            LocationCode = member.LocationCode,
            EmployerCode = member.EmployerCode,
            Gender = member.PersonalDetails.Gender,
            MaritalStatus = member.MaritalStatus,
            IsAvc = isAvc,
            Tenure = member.TenureInYears(),
            NormalRetirementAge = retirementDatesAges?.NormalRetirement(),
            EarliestRetirementAge = retirementDatesAges?.EarliestRetirement(),
            LatestRetirementAge = retirementDatesAges?.GetLatestRetirementAge(),
            TargetRetirementAge = retirementDatesAges?.TargetRetirementAgeYearsIso?.ParseIsoDuration()?.Years,
            NormalRetirementDate = retirementDatesAges?.NormalRetirementDate,
            TargetRetirementDate = retirementDatesAges?.TargetRetirementDate,
            LifeStage = accessKey?.LifeStage,
            RetirementApplicationStatus = accessKey?.RetirementApplicationStatus,
            TransferApplicationStatus = accessKey?.TransferApplicationStatus,
            TenantUrl = accessKey?.TenantUrl,
            HasAdditionalContributions = accessKey?.HasAdditionalContributions,
            DbCalculationStatus = accessKey?.DbCalculationStatus,
            UserId = userId,
            DcRetirementJourney = dcJourneyStatus,
            DcexploreoptionsStarted = accessKey?.WordingFlags?.Any(x => x == MdpConstants.DcJourneyStatus.DcExploreOptionsStarted) ?? false,
            DcretirementapplicationStarted = accessKey?.WordingFlags?.Any(x => x == MdpConstants.DcJourneyStatus.DcRetirementApplicationStarted) ?? false,
            DcretirementapplicationSubmitted = accessKey?.WordingFlags?.Any(x => x == MdpConstants.DcJourneyStatus.DcRetirementApplicationSubmitted) ?? false,
            CurrentAge = accessKey?.CurrentAge,
            DcLifeStage = accessKey?.DcLifeStage
        };
    }
    
    public enum PensionIncome
    {
        Low,
        Medium,
        High
    }
}