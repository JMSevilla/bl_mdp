using System;

namespace WTW.MdpService.Infrastructure;

public static class TimePeriodCalculator
{
    public static (int Years, int month, int Weeks, int Days) Calculate(DateTime startDate, DateTime endDate)
    {
        startDate = startDate.Date;
        endDate = endDate.Date;
        if (startDate > endDate)
            throw new ArgumentException("'Start Date' must be lower that 'End Date'");

        var years = (startDate, endDate) switch
        {
            (DateTime s, DateTime e) when (e.Month > s.Month) => e.Year - s.Year,
            (DateTime s, DateTime e) when (e.Month >= s.Month && e.Day >= s.Day) => e.Year - s.Year,
            _ => (startDate, endDate) switch
            {
                (DateTime s, DateTime e) when (e.Year - 1 - s.Year < 0) => 0,
                _ => endDate.Year - 1 - startDate.Year
            }
        };

        var months = (startDate, endDate) switch
        {
            (DateTime s, DateTime e) when (e.Month >= s.Month && e.Day >= s.Day) => e.Month - s.Month,
            (DateTime s, DateTime e) when (e.Month > s.Month && e.Day < s.Day) => e.Month - 1 - s.Month,
            (DateTime s, DateTime e) when (e.Month == s.Month && e.Day < s.Day) => 11,
            (DateTime s, DateTime e) when (e.Month < s.Month && e.Day >= s.Day) => e.Month + 12 - s.Month,
            (DateTime s, DateTime e) when (e.Month < s.Month && e.Day < s.Day) => e.Month + 12 - 1 - s.Month,
            _ => throw new InvalidOperationException()
        };

        var weeks = (int)Math.Floor((endDate - startDate.AddYears(years).AddMonths(months)).TotalDays / 7);
        var days = (int)(endDate - startDate.AddYears(years).AddMonths(months).AddDays(7 * weeks)).TotalDays;

        return (years, months, weeks, days);
    }
}