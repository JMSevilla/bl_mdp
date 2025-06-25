using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace WTW.MdpService.Infrastructure.MemberDb;

public class MemberDbUnitOfWork : IMemberDbUnitOfWork
{
    private readonly MemberDbContext _context;

    public MemberDbUnitOfWork(MemberDbContext context)
    {
        _context = context;
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    public async Task Commit()
    {
        await _context.SaveChangesAsync();
    }
}