using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb;
public class IfaReferralRepository : IIfaReferralRepository
{
    private readonly MemberDbContext _context;

    public IfaReferralRepository(MemberDbContext context)
    {
        _context = context;
    }

    public async Task<Option<IfaReferral>> Find(string referenceNumber, string businessGroup, string calcType)
    {
        return await _context.IfaReferrals
            .Where(x =>
            x.ReferenceNumber == referenceNumber &&
            x.BusinessGroup == businessGroup &&
            x.CalculationType == calcType)
            .FirstOrDefaultAsync();
    }
}