using System;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Members;

public record RetirementApplicationStatusResponse
{
    public RetirementApplicationStatus RetirementApplicationStatus { get; init; }
    public DateTimeOffset? LatestStartRaDateForSelectedDate { get; init; }
    public DateTimeOffset? EarliestStartRaDateForSelectedDate { get; init; }
    public DateTimeOffset? ExpirationRaDateForSelectedDate { get; init; }
    public MemberLifeStage LifeStage { get; init; }

    public static RetirementApplicationStatusResponse From(
        RetirementApplicationStatus status,
        DateTimeOffset? earliestStartRaDateForSelectedDate,
        DateTimeOffset? latestStartRaDateForSelectedDate,
        DateTimeOffset? expectedRaExpirationDate,
        MemberLifeStage lifeStage)
    {
        return new()
        {
            RetirementApplicationStatus = status,
            EarliestStartRaDateForSelectedDate = earliestStartRaDateForSelectedDate,
            LatestStartRaDateForSelectedDate = latestStartRaDateForSelectedDate,
            ExpirationRaDateForSelectedDate = expectedRaExpirationDate,
            LifeStage = lifeStage
        };
    }

    public static RetirementApplicationStatusResponse From(
        RetirementApplicationStatus status,
        MemberLifeStage lifeStage)
    {
        return new()
        {
            RetirementApplicationStatus = status,
            LifeStage = lifeStage
        };
    }
}