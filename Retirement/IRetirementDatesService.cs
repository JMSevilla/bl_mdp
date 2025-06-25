using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Retirement;

public interface IRetirementDatesService
{
    IEnumerable<(int Age, bool IsTargetRetirementAge)> GetAgeLines(PersonalDetails personalDetails, int targetRetirementAge, DateTimeOffset now);
    string GetFormattedTimeUntilNormalRetirement(Member member, RetirementDatesAges retirementDatesAges, DateTimeOffset utcNow);
    string GetFormattedTimeUntilTargetRetirement(RetirementDatesAges retirementDatesAges, DateTimeOffset utcNow);
    Task<DateTimeOffset?> GetRetirementApplicationExpiryDate(Calculation calculation, Member member, DateTimeOffset now);
}