using System;

namespace WTW.MdpService.BereavementContactsConfirmation;

public class BereavementContactsConfirmationResponse
{
    public DateTimeOffset? TokenExpirationDate { get; init; }

    public static BereavementContactsConfirmationResponse From(DateTimeOffset? tokenExpirationDate)
    {
        return new()
        {
            TokenExpirationDate = tokenExpirationDate,
        };
    }
}

