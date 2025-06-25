namespace WTW.MdpService.Domain.Members;

public enum MemberLifeStage
{
    NotEligibleToRetire = 1,
    PreRetiree = 2,
    EligibleToRetire = 3,
    LateRetirement = 4,
    NewlyRetired = 5,
    EstablishedRetiree = 6,
    EligibleToApplyForRetirement = 7,
    Undefined = 8,
    CloseToLatestRetirementAge = 9,
    OverLatestRetirementAge = 10,
}