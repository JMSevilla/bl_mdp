using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Retirement;

public class RetirementPayTimeline
{
    private ICollection<TenantRetirementTimeline> _timelines;
    private string _category;
    private string _schemeCode;
    private readonly ILogger _logger;

    public RetirementPayTimeline(ICollection<TenantRetirementTimeline> timelines, string category, string schemeCode, ILoggerFactory loggerFactory)
    {
        _timelines = timelines;
        _category = category;
        _schemeCode = schemeCode;
        _logger = loggerFactory.CreateLogger<RetirementPayTimeline>();
    }

    public string PensionPayDay()
    {
        return GetEffectiveTimeline()?.PensionMonthPayDay();
    }

    public string PensionPayDayIndicator()
    {
        return GetEffectiveTimeline()?.PensionMonthPayDayIndicator();
    }

    private TenantRetirementTimeline GetEffectiveTimeline()
    {
        foreach (var timeLine in _timelines)
        {
            if (
                (timeLine.SchemeIdentification == "*" || timeLine.SchemeIdentification.Split(',').Contains(_schemeCode)) &&
                (timeLine.CategoryIdentification == "*" || timeLine.CategoryIdentification.Split(',').Contains(_category))
                )
            {
                return timeLine;
            }
        }

        _logger.LogError("No time timeline record found in \'WW_TIMELINE_CONFIG\' table.");
        return null;
    }
}