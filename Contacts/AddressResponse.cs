using System;
using WTW.MdpService.Domain.Common;

namespace WTW.MdpService.Contacts;

public record MemberAddressResponse
{
    public string StreetAddress1 { get; init; }
    public string StreetAddress2 { get; init; }
    public string StreetAddress3 { get; init; }
    public string StreetAddress4 { get; init; }
    public string StreetAddress5 { get; init; }
    public string Country { get; init; }
    public string CountryCode { get; init; }
    public string PostCode { get; init; }

    public static MemberAddressResponse From(Address address)
    {
        return new()
        {
            StreetAddress1 = address.StreetAddress1,
            StreetAddress2 = address.StreetAddress2,
            StreetAddress3 = address.StreetAddress3,
            StreetAddress4 = address.StreetAddress4,
            StreetAddress5 = address.StreetAddress5,
            Country = address.Country,
            CountryCode = address.CountryCode,
            PostCode = address.PostCode,
        };
    }
}