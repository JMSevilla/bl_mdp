using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Common.Journeys;

namespace WTW.MdpService.Infrastructure.MdpDb;

public class JourneysRepository : IJourneysRepository
{
    private readonly MdpDbContext _context;

    public JourneysRepository(MdpDbContext context)
    {
        _context = context;
    }

    public async Task<Option<GenericJourney>> Find(string businessGroup, string referenceNumber, string type)
    {
        return await _context.Set<GenericJourney>()
             .SingleOrDefaultAsync(j => j.BusinessGroup == businessGroup &&
                                        j.ReferenceNumber == referenceNumber &&
                                        j.Type == type);
    }

    public void Remove(GenericJourney journey)
    {
        _context.Set<GenericJourney>().Remove(journey);
    }

    public async Task Create(GenericJourney journey)
    {
        await _context.AddAsync(journey);
    }

    public async Task<ICollection<GenericJourney>> FindAllMarkedForRemoval(string businessGroup, string referenceNumber)
    {
        return await _context.Set<GenericJourney>()
            .Where(j => j.BusinessGroup == businessGroup && j.ReferenceNumber == referenceNumber && j.IsMarkedForRemoval == true)
            .ToListAsync();
    }

    public void Remove(ICollection<GenericJourney> journeys)
    {
        _context.Set<GenericJourney>().RemoveRange(journeys);
    }

    public async Task<ICollection<GenericJourney>> FindAll(string businessGroup, string referenceNumber)
    {
        return await _context.Set<GenericJourney>()
           .Where(j => j.BusinessGroup == businessGroup && j.ReferenceNumber == referenceNumber)
           .ToListAsync();
    }

    public async Task<ICollection<GenericJourney>> FindAllExpiredUnsubmitted(string businessGroup, string referenceNumber)
    {
        return await _context.Set<GenericJourney>()
           .Where(j =>
                j.BusinessGroup == businessGroup &&
                j.ReferenceNumber == referenceNumber &&
                j.ExpirationDate < DateTimeOffset.UtcNow &&
                j.SubmissionDate == null)
           .ToListAsync();
    }

    public async Task<Option<GenericJourney>> FindUnexpired(string businessGroup, string referenceNumber, string type)
    {
        return await _context.Set<GenericJourney>()
           .SingleOrDefaultAsync(j => j.BusinessGroup == businessGroup &&
                                      j.ReferenceNumber == referenceNumber &&
                                      j.Type == type && j.ExpirationDate > DateTimeOffset.UtcNow);
    }
}