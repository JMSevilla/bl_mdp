using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.Infrastructure.MdpDb;

public class QuoteSelectionJourneyRepository : IQuoteSelectionJourneyRepository
{
    private readonly MdpDbContext _context;

    public QuoteSelectionJourneyRepository(MdpDbContext context)
    {
        _context = context;
    }

    public async Task<Option<QuoteSelectionJourney>> Find(string businessGroup, string referenceNumber)
    {
        return await _context.Set<QuoteSelectionJourney>()
            .SingleOrDefaultAsync(j => j.BusinessGroup == businessGroup &&
                                       j.ReferenceNumber == referenceNumber);
    }

    public void Remove(QuoteSelectionJourney journey)
    {
        _context.Set<QuoteSelectionJourney>().Remove(journey);
    }

    public async Task Add(QuoteSelectionJourney journey)
    {
        await _context.Set<QuoteSelectionJourney>().AddAsync(journey);
    }
}