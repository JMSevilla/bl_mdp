using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Contacts;

public record GetContactCountriesResponse
{
    public string Code { get; init; }
    [JsonPropertyName("dial_code")]
    public string DialCode { get; init; }
    public string Name { get; init; }
    public static IEnumerable<GetContactCountriesResponse> From(IEnumerable<SdDomainList> domainList)
    {
        return domainList.Select(x => new GetContactCountriesResponse
        {
            Code = x.SystemValue,
            Name = x.ListValueDescription,
            DialCode = $"+{x.ListValue}"
        }).ToList();
    }
}
