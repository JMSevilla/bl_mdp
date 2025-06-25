using System;
using System.Collections.Generic;
using System.Linq;

namespace WTW.MdpService.Domain.Members;

public class TenantRetirementTimeline
{
    protected TenantRetirementTimeline() { }

    public TenantRetirementTimeline(int sequenceNumber, string outputId, string @event, string businessGroup, string schemeIdentification, string categoryIdentification, string dateCalculatorFormula)
    {
        SequenceNumber = sequenceNumber;
        OutputId = outputId;
        Event = @event;
        BusinessGroup = businessGroup;
        SchemeIdentification = schemeIdentification;
        CategoryIdentification = categoryIdentification;
        DateCalculatorFormula = dateCalculatorFormula;
    }

    public int SequenceNumber { get; }
    public string OutputId { get; }
    public string Event { get; }
    public string BusinessGroup { get; }
    public string SchemeIdentification { get; }
    public string CategoryIdentification { get; }
    public string DateCalculatorFormula { get; }

    public DateTime FirstMonthlyPensionPayDate(DateTime effectiveRetirementDate, IEnumerable<DateTime> bankHolidays)
    {
        int payDay = int.Parse(DateCalculatorFormula.Split(',')[5]);
        return PayDate(effectiveRetirementDate, bankHolidays, payDay);
    }

    public DateTime LumpSumWithNoAvcPayDate(DateTime effectiveRetirementDate, IEnumerable<DateTime> bankHolidays)
    {
        int daysCount = int.Parse(DateCalculatorFormula.Split(',')[3]);

        var iterator = 0;
        var lumpSumPayDate = effectiveRetirementDate;
        while (iterator < daysCount)
        {
            lumpSumPayDate = lumpSumPayDate.AddDays(1);

            if (IsWorkDay(lumpSumPayDate, bankHolidays))
                iterator++;
        }

        return lumpSumPayDate;
    }

    public DateTime LumpSumWithAvcPayDate(DateTime effectiveRetirementDate, IEnumerable<DateTime> bankHolidays)
    {
        int daysCount = int.Parse(DateCalculatorFormula.Split(',')[3]);
        var payDate = effectiveRetirementDate.AddDays(daysCount);
        return MostRecentPresentPastWorkDay(bankHolidays, payDate);
    }

    public string PensionMonthPayDay()
    {
        return DateCalculatorFormula.Split(',')[5];
    }

    public string PensionMonthPayDayIndicator()
    {
        return DateCalculatorFormula.Split(',').Count() >= 7 ? DateCalculatorFormula.Split(',')[6] : null;
    }

    private bool IsWorkDay(DateTime lumpSumPayDate, IEnumerable<DateTime> bankHolidays)
    {
        return MostRecentPresentPastWorkDay(bankHolidays, lumpSumPayDate).Date == lumpSumPayDate.Date;
    }

    private static DateTime PayDate(DateTime effectiveRetirementDate, IEnumerable<DateTime> bankHolidays, int payDay)
    {
        var payDate = new DateTime(effectiveRetirementDate.AddMonths(1).Year, effectiveRetirementDate.AddMonths(1).Month, payDay);

        return MostRecentPresentPastWorkDay(bankHolidays, payDate);
    }

    private static DateTime MostRecentPresentPastWorkDay(IEnumerable<DateTime> bankHolidays, DateTime date)
    {
        DateTime payDate = date;
        do
        {
            payDate = payDate switch
            {
                DateTime d when bankHolidays.Any(x => x.Date == d.Date) => d.AddDays(-1) switch
                {
                    DateTime dd when dd.DayOfWeek == DayOfWeek.Sunday => dd.AddDays(-2),
                    DateTime dd when d.DayOfWeek == DayOfWeek.Saturday => dd.AddDays(-1),
                    DateTime dd => dd
                },
                DateTime d when d.DayOfWeek == DayOfWeek.Sunday => d.AddDays(-2),
                DateTime d when d.DayOfWeek == DayOfWeek.Saturday => d.AddDays(-1),
                DateTime d => d
            };
        } while (bankHolidays.Any(x => x.Date == payDate.Date));

        return payDate;
    }
}