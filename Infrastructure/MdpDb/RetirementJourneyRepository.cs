using System;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.Infrastructure.MdpDb;
public class RetirementJourneyRepository : IRetirementJourneyRepository
{
    private readonly MdpDbContext _context;

    public RetirementJourneyRepository(MdpDbContext context)
    {
        _context = context;
    }

    public async Task Create(RetirementJourney journey)
    {
        await _context.RetirementJourneys.AddAsync(journey);
    }

    public async Task<Option<RetirementJourney>> Find(string businessGroup, string referenceNumber)
    {
        return await _context.RetirementJourneys
           .SingleOrDefaultAsync(j => j.BusinessGroup == businessGroup &&
                                      j.ReferenceNumber == referenceNumber);
    }

    public async Task<Option<RetirementJourney>> FindUnexpiredOrSubmittedJourney(string businessGroup, string referenceNumber, DateTimeOffset now)
    {
        return await _context.RetirementJourneys
            .SingleOrDefaultAsync(j => j.BusinessGroup == businessGroup &&
                                       j.ReferenceNumber == referenceNumber &&
                                       (j.ExpirationDate > now || j.SubmissionDate != null));
    }

    public async Task<Option<RetirementJourney>> FindUnexpiredJourney(string businessGroup, string referenceNumber, DateTimeOffset now)
    {
        return await _context.RetirementJourneys
            .SingleOrDefaultAsync(j => j.BusinessGroup == businessGroup &&
                j.ReferenceNumber == referenceNumber &&
                j.ExpirationDate > now);
    }

    public async Task<Option<RetirementJourney>> FindUnexpiredUnsubmittedJourney(string businessGroup, string referenceNumber, DateTimeOffset now)
    {
        return await _context.RetirementJourneys
            .SingleOrDefaultAsync(j => j.BusinessGroup == businessGroup &&
                                       j.ReferenceNumber == referenceNumber &&
                                       j.ExpirationDate > now && j.SubmissionDate == null);
    }

    public async Task<Option<RetirementJourney>> FindExpiredJourney(string businessGroup, string referenceNumber)
    {
        return await _context.RetirementJourneys
            .SingleOrDefaultAsync(j => j.BusinessGroup == businessGroup &&
                                       j.ReferenceNumber == referenceNumber &&
                                       j.ExpirationDate <= System.DateTimeOffset.UtcNow);
    }

    public void Remove(RetirementJourney journey)
    {
        _context.RetirementJourneys.Remove(journey);
    }
}