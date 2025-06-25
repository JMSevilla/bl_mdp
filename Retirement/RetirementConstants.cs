namespace WTW.MdpService.Retirement;

public class RetirementConstants
{
    public const int RetirementProcessingPeriodInDays = 30 + RetirementSubmissionMinimumWindowInDays;
    public const int RetirementApplicationPeriodInMonths = 6;
    public const int RetirementApplicationPeriodInDaysRBS = 90;
    public const int LatestRetirementAgeInYears = 75;
    public const double OverLatestRetirementAgePeriodInMonth = 1.24;
    public const int RetirementConfirmationInDays = 14;
    public const int RetirementSubmissionMinimumWindowInDays = 7;
    public const int DcRetirementDateAdditionalPeriodInMonth = 6;
    public const int DcRetirementJourneyExpiryInDays = 90;
    public const int DcRetirementProcessingPeriodInDays = 21;
    public const int DbCoreRetirementJourneyExpiryInDays = 90;
    public const int GMPOnlyMaleMemberRetirementAgeInYears = 65;
    public const int GMPOnlyFemaleMemberRetirementAgeInYears = 60;
    public const int CrownDependencyMemberMinimumPensiontAgeInYears = 55;
    public const int CrownDependencyGSYOrJSYMemberMaximumPensionAgeInYears = 75;
    public const int CrownDependencyIOMMemberMaximumPensionAgeInYears = 70;

    public enum MemberJuridiction
    {
        JSY,
        GSY,
        IOM
    }

    public enum Gender
    {
        M,
        F
    }
}