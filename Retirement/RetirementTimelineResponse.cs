using System;

namespace WTW.MdpService.Retirement;

public record RetirementTimelineResponse
{
    public DateTime RetirementDate { get; init; }
    public DateTimeOffset? EarliestStartRaDateForSelectedDate { get; init; }
    public DateTimeOffset LatestStartRaDateForSelectedDate { get; init; }
    public DateTime RetirementConfirmationDate { get; init; }
    public DateTime FirstMonthlyPensionPayDate { get; init; }
    public DateTime LumpSumPayDate { get; init; }

    public static RetirementTimelineResponse From(DateTime effectiveRetirementDate,
        DateTimeOffset? earliestStartRaDateForSelectedDate,
        DateTimeOffset latestStartRaDateForSelectedDate,
        DateTime retirementConfirmationDate,
        DateTime firstMonthlyPensionPayDate,
        DateTime lumpSumPayDate)
    {
        return new()
        {
            RetirementDate = effectiveRetirementDate,
            EarliestStartRaDateForSelectedDate = earliestStartRaDateForSelectedDate,
            LatestStartRaDateForSelectedDate = latestStartRaDateForSelectedDate,
            RetirementConfirmationDate = retirementConfirmationDate,
            FirstMonthlyPensionPayDate = firstMonthlyPensionPayDate,
            LumpSumPayDate = lumpSumPayDate
        };
    }
}