using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace WTW.MdpService.Infrastructure.BereavementDb;
public class BereavementUnitOfWork : IBereavementUnitOfWork
{
    private readonly BereavementDbContext _context;
    private readonly ILogger<BereavementUnitOfWork> _logger;

    public BereavementUnitOfWork(BereavementDbContext context, ILogger<BereavementUnitOfWork> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Commit()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            throw;
        }
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        try
        {
            return await _context.Database.BeginTransactionAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            throw;
        }
    }
}