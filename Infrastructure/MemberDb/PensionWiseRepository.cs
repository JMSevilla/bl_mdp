using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb;

public class PensionWiseRepository
{
    private readonly MemberDbContext _context;

    public PensionWiseRepository(MemberDbContext context)
    {
        _context = context;
    }
    
    public ValueTask<EntityEntry<PensionWise>> AddAsync(PensionWise pensionWise)
    {
        return _context.PensionWises.AddAsync(pensionWise);
    }
}