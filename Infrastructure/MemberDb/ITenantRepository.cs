using System.Collections.Generic;
using System.Threading.Tasks;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb;

public interface ITenantRepository
{
    Task<string> GetBusinessGroupStatus(string businessGroup);
    Task<ICollection<SdDomainList>> ListRelationships(string businessGroup);
    Task<IEnumerable<SdDomainList>> GetAddressCountries(string domain, string bGroup);
}