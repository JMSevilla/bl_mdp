using System;
using System.Linq;
using System.Threading.Tasks;

namespace WTW.MdpService.Infrastructure.MemberDb;

public class SystemRepository : ISystemRepository
{
    private readonly MemberDbContext _context;

    public SystemRepository(MemberDbContext context)
    {
        _context = context;
    }

    public async Task<long> NextAuthorizationNumber()
    {
        return (await _context.Database.ExecuteQuery<long>(
            "SELECT pms_ps_gen.get_next_authno FROM DUAL",
            read => Convert.ToInt64(read["GET_NEXT_AUTHNO"]))).First();
    }

    public async Task<long> NextAddressNumber()
    {
        return (await _context.Database.ExecuteQuery<long>(
            "SELECT PS_ADDNOGEN.nextval FROM DUAL",
            read => Convert.ToInt64(read["NEXTVAL"]))).First();
    }
}