using System;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Mdp.Calculations;

namespace WTW.MdpService.Infrastructure.MdpDb;

public class CalculationsRepository : ICalculationsRepository
{
    private readonly MdpDbContext _context;

    public CalculationsRepository(MdpDbContext context)
    {
        _context = context;
    }

    public async Task<Option<Calculation>> FindWithJourney(string referenceNumber, string businessGroup)
    {
        return await _context.Calculations
            .Include(x => x.RetirementJourney)
            .FirstOrDefaultAsync(c => c.BusinessGroup == businessGroup &&
            c.ReferenceNumber == referenceNumber && c.RetirementJourney != null);
    }

    public async Task<Option<Calculation>> FindWithValidRetirementJourney(string referenceNumber, string businessGroup, DateTimeOffset now)
    {
        return await _context.Calculations
            .Include(x => x.RetirementJourney)
            .FirstOrDefaultAsync(c => c.BusinessGroup == businessGroup &&
            c.ReferenceNumber == referenceNumber &&
            c.RetirementJourney != null &&
            (c.RetirementJourney.ExpirationDate > now || c.RetirementJourney.SubmissionDate != null));
    }

    public async Task<bool> ExistsWithUnexpiredRetirementJourney(string referenceNumber, string businessGroup, DateTimeOffset now)
    {
        return _context.Calculations
            .Include(x => x.RetirementJourney)
            .Exists(c => c.BusinessGroup == businessGroup &&
                c.ReferenceNumber == referenceNumber &&
                c.RetirementJourney != null &&
                c.RetirementJourney.ExpirationDate > now);
    }

    public async Task<Option<Calculation>> Find(string referenceNumber, string businessGroup)
    {
        return await _context.Calculations
            .Include(x => x.RetirementJourney)
            .FirstOrDefaultAsync(c => c.BusinessGroup == businessGroup && c.ReferenceNumber == referenceNumber);
    }

    public async Task Create(Calculation calculation)
    {
        await _context.Calculations.AddAsync(calculation);
    }

    public void Remove(Calculation calculation)
    {
        _context.Calculations.Remove(calculation);
    }
}