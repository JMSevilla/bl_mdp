using System;

namespace WTW.MdpService.BereavementJourneys;

public record BereavementJourneyKeepAliveResponse
{
    public DateTimeOffset ExpirationDate { get; init; }

    public static BereavementJourneyKeepAliveResponse From(DateTimeOffset expirationDate)
    {
        return new()
        {
            ExpirationDate = expirationDate
        };
    }
}