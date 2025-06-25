using System.Collections.Generic;
using System.Linq;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Addresses;

public record GetCountriesResponse
{
    public string Code { get; init; }
    public string Name { get; init; }
    public static IEnumerable<GetCountriesResponse> From(IEnumerable<SdDomainList> domainList)
    {
        return domainList.Select(x => new GetCountriesResponse
        {
            Code = x.ListValue,
            Name = x.ListValueDescription
        }).ToList();
    }

}
