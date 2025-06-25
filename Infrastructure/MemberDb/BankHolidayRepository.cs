using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb;

public class BankHolidayRepository : IBankHolidayRepository
{
    private readonly MemberDbContext _context;

    public BankHolidayRepository(MemberDbContext context)
    {
        _context = context;
    }

    public async Task<ICollection<BankHoliday>> ListFrom(DateTime fromDate)
    {
        return await _context.Set<BankHoliday>()
            .Where(x => x.Date >= fromDate)
            .ToListAsync();
    }
}