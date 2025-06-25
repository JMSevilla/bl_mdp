using System;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.Infrastructure.MdpDb;

public class TransferJourneyRepository : ITransferJourneyRepository
{
    private readonly MdpDbContext _context;

    public TransferJourneyRepository(MdpDbContext context)
    {
        _context = context;
    }

    public async Task<Option<TransferJourney>> Find(string businessGroup, string referenceNumber)
    {
        return await _context.Set<TransferJourney>()
            .SingleOrDefaultAsync(j => j.BusinessGroup == businessGroup &&
                                       j.ReferenceNumber == referenceNumber);
    }

    public void Remove(TransferJourney journey)
    {
        _context.Set<TransferJourney>().Remove(journey);
    }

    public async Task Create(TransferJourney journey)
    {
        await _context.Set<TransferJourney>().AddAsync(journey);
    }
}