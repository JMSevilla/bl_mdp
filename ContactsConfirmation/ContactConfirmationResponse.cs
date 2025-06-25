using System;

namespace WTW.MdpService.ContactsConfirmation;

public record ContactConfirmationResponse
{
    public DateTimeOffset TokenExpirationDate { get; init; }

    public static ContactConfirmationResponse From(DateTimeOffset tokenExpirationDate)
    {
        return new()
        {
            TokenExpirationDate = tokenExpirationDate
        };
    }
}