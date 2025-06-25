using System;

namespace WTW.MdpService.Domain.Common.Journeys;

public record GenericJourneyStageStatus
{
    public GenericJourneyStageStatus(string stage, DateTime? completedDate, bool inProgress, string firstPageKey = null)
    {
        Stage = stage;
        CompletedDate = completedDate;
        InProgress = inProgress;
        FirstPageKey = firstPageKey;
    }

    public string Stage { get; }
    public DateTime? CompletedDate { get; }
    public bool InProgress { get; }
    public string FirstPageKey { get; }
}