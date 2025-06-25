using System.Collections.Generic;
using System.Linq;

namespace WTW.MdpService.Infrastructure.Geolocation;

public class LocationApiAddressDetailsResponse
{
    public IEnumerable<LocationApiAddressDetails> Items { get; set; }
    public bool IsSuccess => Items.All(x => x.IsSuccess);
    public IEnumerable<string> Errors => Items.Select(x => $"Error: {x.Error} Cause: {x.Cause} Description: {x.Description}");
}

public class LocationApiAddressDetails : LocationApiBaseResponse
{
    public string Id { get; set; }
    public string City { get; set; }
    public string Line1 { get; set; }
    public string Line2 { get; set; }
    public string Line3 { get; set; }
    public string Line4 { get; set; }
    public string Line5 { get; set; }
    public string PostalCode { get; set; }
    public string CountryIso2 { get; set; }
    public string Type { get; set; }
}