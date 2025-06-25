using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Domain.Members.Beneficiaries;

namespace WTW.MdpService.Infrastructure.MemberDb;

public class TenantRepository : ITenantRepository
{
    private readonly MemberDbContext _context;

    public TenantRepository(MemberDbContext context)
    {
        _context = context;
    }

    public async Task<ICollection<SdDomainList>> ListRelationships(string businessGroup)
    {
        var relationshipDomain = "NOM01";

        var bgroupSpecific = await GetRelationshipDomainList(businessGroup, relationshipDomain);

        if (bgroupSpecific.Any())
        {
            return bgroupSpecific;
        }

        return await GetRelationshipDomainList("ZZY", relationshipDomain);
    }

    private async Task<ICollection<SdDomainList>> GetRelationshipDomainList(string businessGroup, string domain)
    {
        return await _context.Set<SdDomainList>()
            .Where(x => x.BusinessGroup == businessGroup &&
                        x.Domain == domain &&
                        x.ListValue != BeneficiaryDetails.CharityStatus)
            .ToListAsync();
    }

    public async Task<string> GetBusinessGroupStatus(string businessGroup)
    {
        return await _context.Set<SdDomainList>()
            .Where(x => x.BusinessGroup == businessGroup && x.Domain == "MF2FA")
            .Select(x => x.ListValue)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<SdDomainList>> GetAddressCountries(string domain, string bGroup)
    {
        return await _context.Set<SdDomainList>()
            .Where(x => x.Domain == domain && x.BusinessGroup == bGroup)
            .ToListAsync();
    }
}