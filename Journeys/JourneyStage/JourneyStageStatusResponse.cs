using System;
using WTW.MdpService.Domain.Common.Journeys;

namespace WTW.MdpService.Journeys;

public class JourneyStageStatusResponse
{
    public JourneyStageStatusResponse(GenericJourneyStageStatus status)
    {
        Stage = status.Stage;
        CompletedDate = status.CompletedDate;
        InProgress = status.InProgress;
        FirstPageKey = status.FirstPageKey;
    }

    public string Stage { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool InProgress { get; set; }
    public string FirstPageKey { get; set; }
}