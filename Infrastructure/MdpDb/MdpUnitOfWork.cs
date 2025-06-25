using System;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace WTW.MdpService.Infrastructure.MdpDb;

public class MdpUnitOfWork : IMdpUnitOfWork
{
    private readonly MdpDbContext _context;
    private readonly ILogger<MdpUnitOfWork> _logger;

    public MdpUnitOfWork(MdpDbContext context, ILogger<MdpUnitOfWork> logger)
    {
        _context = context;
        _logger = logger;
    }

    public MdpDbContext Context => _context;

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

    public void Remove(object obj)
    {
        _context.Remove(obj);
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

    public async Task<Either<string, bool>> TryCommit()
    {
        try
        {
            return await _context.SaveChangesAsync() > 0;
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException != null && ex.InnerException.Message.Contains("AK_RetirementJourney_BusinessGroup_ReferenceNumber"))
                return "Record exist";

            throw;
        }
    }
}