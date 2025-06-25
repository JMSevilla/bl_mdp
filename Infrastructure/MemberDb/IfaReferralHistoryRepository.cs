using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb;
public class IfaReferralHistoryRepository : IIfaReferralHistoryRepository
{
    private readonly MemberDbContext _context;

    public IfaReferralHistoryRepository(MemberDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<IfaReferralHistory>> Find(string referenceNumber, string businessGroup)
    {
        return await _context.IfaReferralHistories
            .Where(x => x.ReferenceNumber == referenceNumber && x.BusinessGroup == businessGroup)
            .ToListAsync();
    }
}