using System;
using System.Collections.Generic;
using System.Linq;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Retirement;

public class RetirementPayDates
{
    private readonly ICollection<TenantRetirementTimeline> _timelines;
    private readonly ICollection<BankHoliday> _bankHolidays;
    private readonly string _businessGroup;
    private readonly DateTime _effectiveRetirementDate;

    public RetirementPayDates(ICollection<TenantRetirementTimeline> timelines, ICollection<BankHoliday> bankHolidays, string businessGroup, DateTime effectiveRetirementDate)
    {
        _timelines = timelines;
        _bankHolidays = bankHolidays;
        _businessGroup = businessGroup;
        _effectiveRetirementDate = effectiveRetirementDate;
    }

    public DateTime FirstMonthlyPensionPayDate()
    {
        var timeLine =
            _timelines
                .Where(x => x.BusinessGroup == _businessGroup && x.OutputId == "RETFIRSTPAYMADE")
                .OrderByDescending(x => x.SequenceNumber)
                .FirstOrDefault()
            ?? _timelines
                .Where(x => x.BusinessGroup == "ZZY" && x.OutputId == "RETFIRSTPAYMADE")
                .OrderByDescending(x => x.SequenceNumber)
                .First();

        return timeLine.FirstMonthlyPensionPayDate(_effectiveRetirementDate, _bankHolidays.Select(x => x.Date));
    }

    public DateTime LumpSumPayDate(bool isAvc)
    {
        if (isAvc)
            return _timelines
                    .Where(x => x.BusinessGroup == _businessGroup && x.OutputId == "RETAVCSLSRECD")
                    .OrderByDescending(x => x.SequenceNumber)
                    .FirstOrDefault()
                    ?.LumpSumWithAvcPayDate(_effectiveRetirementDate, _bankHolidays.Select(x => x.Date))
                ??
                _timelines
                    .Where(x => x.BusinessGroup == "ZZY" && x.OutputId == "RETAVCSLSRECD")
                    .OrderByDescending(x => x.SequenceNumber)
                    .First()
                    .LumpSumWithAvcPayDate(_effectiveRetirementDate, _bankHolidays.Select(x => x.Date));
        return
            _timelines
                .Where(x => x.BusinessGroup == _businessGroup && x.OutputId == "RETLSRECD")
                .OrderByDescending(x => x.SequenceNumber)
                .FirstOrDefault()
                ?.LumpSumWithNoAvcPayDate(_effectiveRetirementDate, _bankHolidays.Select(x => x.Date))
            ??
            _timelines
                .Where(x => x.BusinessGroup == "ZZY" && x.OutputId == "RETLSRECD")
                .OrderByDescending(x => x.SequenceNumber)
                .First()
                .LumpSumWithNoAvcPayDate(_effectiveRetirementDate, _bankHolidays.Select(x => x.Date));
    }
}