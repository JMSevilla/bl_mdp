using System;

namespace WTW.MdpService.BereavementJourneys;

public record BereavementJourneySubmitResponse
{
    public string CaseNumber { get; init; }

    public static BereavementJourneySubmitResponse From(string caseNumber)
    {
        return new() { CaseNumber = caseNumber };
    }
}
