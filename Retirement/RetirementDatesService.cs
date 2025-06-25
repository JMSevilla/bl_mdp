using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.RetirementJourneys;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Retirement;

public class RetirementDatesService : IRetirementDatesService
{
    private readonly IJourneysRepository _journeysRepository;
    private readonly RetirementJourneyConfiguration _retirementJourneyConfiguration;

    public RetirementDatesService() { }

    public RetirementDatesService(IJourneysRepository journeysRepository, RetirementJourneyConfiguration retirementJourneyConfiguration)
    {
        _journeysRepository = journeysRepository;
        _retirementJourneyConfiguration = retirementJourneyConfiguration;
    }

    public string GetFormattedTimeUntilNormalRetirement(Member member, RetirementDatesAges retirementDatesAges, DateTimeOffset utcNow)
    {
        var latestRetirementDate = member.LatestRetirementDate(retirementDatesAges.LatestRetirementDate, retirementDatesAges.GetLatestRetirementAge(), member.BusinessGroup, utcNow);
        var normalRetirementDate = retirementDatesAges.NormalRetirement(member.BusinessGroup, utcNow, latestRetirementDate).Date;
        if (normalRetirementDate.Date < utcNow.Date)
            normalRetirementDate = utcNow.Date;

        return FormatToIsoDuration(utcNow, normalRetirementDate);
    }

    public string GetFormattedTimeUntilTargetRetirement(RetirementDatesAges retirementDatesAges, DateTimeOffset utcNow)
    {
        var targetRetirement = retirementDatesAges.TargetRetirementDate.HasValue ? retirementDatesAges.TargetRetirementDate.Value.Date : utcNow.Date;
        if (targetRetirement.Date < utcNow.Date)
            targetRetirement = utcNow.Date;

        return FormatToIsoDuration(utcNow, targetRetirement);
    }

    private static string FormatToIsoDuration(DateTimeOffset utcNow, DateTime retirementDate)
    {
        var timeToRetirement = TimePeriodCalculator.Calculate(utcNow.Date, retirementDate);
        return $"{timeToRetirement.Years}Y{timeToRetirement.month}M{timeToRetirement.Weeks}W{timeToRetirement.Days}D";
    }

    public async Task<DateTimeOffset?> GetRetirementApplicationExpiryDate(Calculation calculation, Member member, DateTimeOffset utcNow)
    {
        if ("DC".Equals(member.Scheme?.Type, StringComparison.InvariantCultureIgnoreCase))
        {
            var journey = await _journeysRepository.Find(member.BusinessGroup, member.ReferenceNumber, "dcretirementapplication");

            if (journey.IsNone)
                return null;

            return journey.Value().ExpirationDate;
        }

        return calculation.ExpectedRetirementJourneyExpirationDate(utcNow, _retirementJourneyConfiguration.RetirementJourneyDaysToExpire);
    }

    public IEnumerable<(int Age, bool IsTargetRetirementAge)> GetAgeLines(PersonalDetails personalDetails, int targetRetirementAge, DateTimeOffset now)
    {
        var lines = new List<(int, bool)>();
        var targetDate = personalDetails.DateOfBirth.Value.Date.AddYears(targetRetirementAge);
        if (targetDate < now)
            return lines;

        var start = personalDetails.DateOfBirth.Value;
        bool isTargetRetirementAge = true;
        for (int i = 0; i < 8; i++)
        {
            if (targetDate < now.AddYears(1))
                break;

            var age = (targetDate.Year - start.Year - 1) + (((targetDate.Month > start.Month) || ((targetDate.Month == start.Month) && (targetDate.Day >= start.Day))) ? 1 : 0);
            lines.Add((age, isTargetRetirementAge));

            targetDate = targetDate.AddYears(-1);
            isTargetRetirementAge = false;
        }

        return lines.OrderBy(x => x);
    }
}