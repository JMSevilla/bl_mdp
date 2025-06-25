using System;

namespace WTW.MdpService.Domain.Members;

public class BankHoliday
{
    protected BankHoliday() { }

    public BankHoliday(DateTime date, string description)
    {
        Date = date;
        Description = description;
    }

    public DateTime Date { get; }
    public string Description { get; }
}