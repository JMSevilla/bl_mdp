using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WTW.MdpService.Infrastructure.MemberDb;

public class IdvDetailRepository
{
    private readonly MemberDbContext _context;

    public IdvDetailRepository(MemberDbContext context)
    {
        _context = context;
    }

    public async Task<int> NextId()
    {
        // return await _context.IdvDetails.MaxAsync(x => (int?)x.Id) ?? 0;
        return (await _context.Database.ExecuteQuery(
            "SELECT WWIDVHDR_SEQ.nextval FROM DUAL",
            read => Convert.ToInt32(read["NEXTVAL"]))).First();
    }
}