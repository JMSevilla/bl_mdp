using WTW.MdpService.Infrastructure.Geolocation;

namespace WTW.MdpService.BereavementJourneys;

public class BereavementAddressResponse
{
    public string AddressId { get; init; }
    public string City { get; init; }
    public string Line1 { get; init; }
    public string Line2 { get; init; }
    public string Line3 { get; init; }
    public string Line4 { get; init; }
    public string Line5 { get; init; }
    public string CountryIso2 { get; init; }
    public string PostalCode { get; init; }
    public string Type { get; init; }

    public static BereavementAddressResponse From(LocationApiAddressDetails addressDetails)
    {
        return new()
        {
            City = addressDetails.City,
            Line1 = addressDetails.Line1,
            Line2 = addressDetails.Line2,
            Line3 = addressDetails.Line3,
            Line4 = addressDetails.Line4,
            Line5 = addressDetails.Line5,
            AddressId = addressDetails.Id,
            CountryIso2 = addressDetails.CountryIso2,
            PostalCode = addressDetails.PostalCode,
            Type = addressDetails.Type
        };
    }
}