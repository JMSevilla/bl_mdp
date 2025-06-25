using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WTW.MdpService.Infrastructure.MemberDb;

public class ObjectStatusRepository : IObjectStatusRepository
{
    private readonly MemberDbContext _context;

    public ObjectStatusRepository(MemberDbContext context)
    {
        _context = context;
    }

    public async Task<string> FindTableShort(string businessGroup)
    {
        var businessGroups = new List<string>() { "ZZZ", "ZZY", businessGroup };
        businessGroup = await _context.ObjectStatuses
            .Where(x => x.ObjectId == "PSF17" && businessGroups.Contains(x.BusinessGroup))
            .Select(x => x.BusinessGroup)
            .MinAsync();

        return await _context.ObjectStatuses
            .Where(x => x.ObjectId == "PSF17" && x.BusinessGroup == businessGroup)
            .Select(x => x.TableShort)
            .FirstAsync();
    }
}