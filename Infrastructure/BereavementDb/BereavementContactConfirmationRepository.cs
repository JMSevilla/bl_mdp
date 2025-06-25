using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WTW.MdpService.BereavementContactsConfirmation;
using WTW.MdpService.Domain.Bereavement;
using WTW.MdpService.Domain;

namespace WTW.MdpService.Infrastructure.BereavementDb;

public class BereavementContactConfirmationRepository : IBereavementContactConfirmationRepository
{
    private readonly BereavementDbContext _context;

    public BereavementContactConfirmationRepository(BereavementDbContext context)
    {
        _context = context;
    }

    public async Task<Option<BereavementContactConfirmation>> FindLastEmailConfirmation(string businessGroup, Guid referenceNumber)
    {
        return await _context.Set<BereavementContactConfirmation>()
            .Where(t => t.BusinessGroup == businessGroup &&
                t.ReferenceNumber == referenceNumber.ToString())
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task Create(BereavementContactConfirmation token)
    {
        await _context.Set<BereavementContactConfirmation>().AddAsync(token);
    }

    public void Remove(BereavementContactConfirmation confirmation)
    {
        _context.Set<BereavementContactConfirmation>().Remove(confirmation);
    }

    public void Remove(IEnumerable<BereavementContactConfirmation> confirmations)
    {
        _context.Set<BereavementContactConfirmation>().RemoveRange(confirmations);
    }

    public async Task<IEnumerable<BereavementContactConfirmation>> FindExpiredUnlocked(DateTimeOffset utcNow, int emailLockPeriodInMin)
    {
        return await _context.Set<BereavementContactConfirmation>()
            .FromSqlRaw($"SELECT * FROM \"BereavementContactConfirmation\" " +
                $"WHERE \"ExpiresAt\" <= @utcNow AND " +
                    $"(\"FailedConfirmationAttemptCount\" < \"MaximumConfirmationAttemptCount\" OR \"CreatedAt\" <= @lockReleaseDate ) " +
                $"FOR UPDATE",
                new NpgsqlParameter("utcNow", utcNow),
                new NpgsqlParameter("lockReleaseDate", utcNow.AddMinutes(-emailLockPeriodInMin)))
            .ToListAsync();
    }

    public async Task<Option<BereavementContactConfirmation>> FindLocked(Email email)
    {
        var hashedEmail = EmailSecurity.Hash(email);
        return await _context.Set<BereavementContactConfirmation>()
            .FirstOrDefaultAsync(c => c.Contact == hashedEmail && c.FailedConfirmationAttemptCount >= c.MaximumConfirmationAttemptCount);
    }
}