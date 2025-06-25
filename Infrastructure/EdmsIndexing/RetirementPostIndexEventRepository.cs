using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Infrastructure.Db;

namespace WTW.MdpService.Infrastructure.MdpDb;

public class RetirementPostIndexEventRepository : IRetirementPostIndexEventRepository
{
    private readonly MdpDbContext _context;
    private readonly IDatabaseConnectionParser _dbConfigProvider;
    private readonly ILogger<RetirementPostIndexEventRepository> _logger;

    public RetirementPostIndexEventRepository(MdpDbContext context, 
        IDatabaseConnectionParser dbConfigProvider,
        ILogger<RetirementPostIndexEventRepository> logger)
    {
        _context = context;
        _dbConfigProvider = dbConfigProvider;
        _logger = logger;
    }

    public async Task<List<RetirementPostIndexEvent>> List()
    {
        var currentSid = _dbConfigProvider.GetSid();
        _logger.LogInformation("Current SID: {CurrentSid}", currentSid);
        if (currentSid == null)
        {
            _logger.LogWarning("Current SID is null. Returning an empty list");
            return new List<RetirementPostIndexEvent>();
        }
        
        return await _context.RetirementPostIndexEvent.FromSqlInterpolated($"SELECT * FROM \"RetirementPostIndexEvent\" WHERE \"DbId\" = {currentSid} FOR UPDATE")
            .Where(postIndex => string.IsNullOrEmpty(postIndex.Error))
            .ToListAsync();
    }

    public void Add(RetirementPostIndexEvent ev)
    {
        var sid = _dbConfigProvider.GetSid();
        _logger.LogInformation("Setting SID to {Sid} for the new event as DbId", sid);
        ev.SetDbId(sid);
        _context.RetirementPostIndexEvent.Add(ev);
    }

    public void Delete(RetirementPostIndexEvent ev)
    {
        _context.RetirementPostIndexEvent.Remove(ev);
    }
}