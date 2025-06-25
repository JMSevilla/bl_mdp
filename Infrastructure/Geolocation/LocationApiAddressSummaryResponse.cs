using System.Collections.Generic;
using System.Linq;

namespace WTW.MdpService.Infrastructure.Geolocation;

public class LocationApiAddressSummaryResponse
{
    public IEnumerable<LocationApiAddressSummary> Items { get; set; }
    public bool IsSuccess => Items.All(x => x.IsSuccess);
    public IEnumerable<string> Errors => Items.Select(x => $"Error: {x.Error} Cause: {x.Cause} Description: {x.Description}");
}

public class LocationApiAddressSummary : LocationApiBaseResponse
{
    public string Highlight { get; set; }
    public string Id { get; set; }
    public string Text { get; set; }
    public string Type { get; set; }
}
