using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WTW.MdpService.Infrastructure.MemberDb;

public class EventRtiRepository
{
    private readonly MemberDbContext _context;

    public EventRtiRepository(MemberDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetScoreSum(string businessGroup, string caseCode)
    {
        var sum = await _context.EventRtis.Where(x => x.BusinessGroup == businessGroup 
                                                                         && x.CaseCode == caseCode
                                                                         && x.Status == "AC")
            .SumAsync(x => x.Score);

        return sum;
    }
}