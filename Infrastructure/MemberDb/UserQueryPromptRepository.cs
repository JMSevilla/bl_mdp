using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WTW.MdpService.Infrastructure.MemberDb;

public class UserQueryPromptRepository
{
    private readonly MemberDbContext _context;

    public UserQueryPromptRepository(MemberDbContext context)
    {
        _context = context;
    }

    public async Task<int?> FindScoreNumber(string businessGroup, string caseCode, string @event, string memberStatus)
    {
        var uqp = await _context.UserQueryPrompts.SingleOrDefaultAsync(x => x.BusinessGroup == businessGroup 
                                                                         && x.CaseCode == caseCode 
                                                                         && x.Event == @event 
                                                                         && x.Status == memberStatus);
        return uqp?.ScoreNumber;
    }
}