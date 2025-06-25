namespace WTW.MdpService.Members;

public record TimePeriodResponse
{   
    public TimeToDate TimeToNormalRetirement { get; init; }
    public TimeToDate AgeAtNormalRetirement { get; init; }
    public TimeToDate CurrentAge { get; init; }

    public static TimePeriodResponse From(
        (int Years, int month, int Weeks, int Days) periodToNormalRetirementDate,
        (int Years, int month, int Weeks, int Days) ageAtRetirementNormalRetirementDate,
        (int Years, int month, int Weeks, int Days) currentAge)
    {
        return new()
        {         
            TimeToNormalRetirement = new()
            {
                Years = periodToNormalRetirementDate.Years,
                Months = periodToNormalRetirementDate.month,
                Weeks = periodToNormalRetirementDate.Weeks,
                Days = periodToNormalRetirementDate.Days,
            },
            AgeAtNormalRetirement = new()
            {
                Years = ageAtRetirementNormalRetirementDate.Years,
                Months = ageAtRetirementNormalRetirementDate.month,
                Weeks = ageAtRetirementNormalRetirementDate.Weeks,
                Days = ageAtRetirementNormalRetirementDate.Days,
            },
            CurrentAge = new()
            {
                Years = currentAge.Years,
                Months = currentAge.month,
                Weeks = currentAge.Weeks,
                Days = currentAge.Days,
            }
        };
    }

    public record TimeToDate()
    {
        public int Years { get; init; }
        public int Months { get; init; }
        public int Weeks { get; init; }
        public int Days { get; init; }
    }
}