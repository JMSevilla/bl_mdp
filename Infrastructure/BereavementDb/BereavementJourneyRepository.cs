using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WTW.MdpService.Domain.Bereavement;
using WTW.MdpService.Infrastructure.MdpDb;

namespace WTW.MdpService.Infrastructure.BereavementDb;

public class BereavementJourneyRepository : IBereavementJourneyRepository
{
    private readonly BereavementDbContext _context;

    public BereavementJourneyRepository(BereavementDbContext context)
    {
        _context = context;
    }

    public async Task<Option<BereavementJourney>> Find(string businessGroup, Guid bereavmentReferenceNumber)
    {
        return await _context.Set<BereavementJourney>()
            .SingleOrDefaultAsync(j => j.BusinessGroup == businessGroup &&
                                       j.ReferenceNumber == bereavmentReferenceNumber.ToString());
    }

    public async Task<IEnumerable<BereavementJourney>> FindExpired(DateTimeOffset utcNow)
    {
        var parameter = new NpgsqlParameter("utcNow", utcNow);
        return await _context.Set<BereavementJourney>()
            .FromSqlRaw($"SELECT * FROM \"BereavementJourney\" WHERE \"ExpirationDate\" <= @utcNow FOR UPDATE", parameter)
            .ToListAsync();
    }

    public async Task<Option<BereavementJourney>> FindUnexpired(string businessGroup, Guid bereavmentReferenceNumber, DateTimeOffset utcNow)
    {
        return await _context.Set<BereavementJourney>()
            .SingleOrDefaultAsync(j => j.BusinessGroup == businessGroup &&
                                       j.ReferenceNumber == bereavmentReferenceNumber.ToString() &&
                                       //we need to add a buffer because of lag between api and client
                                       j.ExpirationDate.AddSeconds(60) > utcNow);
    }

    public void Remove(BereavementJourney journey)
    {
        _context.Set<BereavementJourney>().Remove(journey);
    }

    public void Remove(IEnumerable<BereavementJourney> journeys)
    {
        _context.Set<BereavementJourney>().RemoveRange(journeys);
    }

    public async Task Create(BereavementJourney journey)
    {
        await _context.Set<BereavementJourney>().AddAsync(journey);
    }
}