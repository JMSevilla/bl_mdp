using System.Collections.Generic;
using System.Linq;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Tenants;

public record RelationshipStatusesResponse
{
    public IEnumerable<string> Statuses { get; init; }

    public static RelationshipStatusesResponse From(ICollection<SdDomainList> statuses)
    {
        return new()
        {
            Statuses = statuses.Select(x => x.ListValue)
        };
    }
}