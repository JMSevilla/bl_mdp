using System;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Mdp.Calculations;

namespace WTW.MdpService.Infrastructure.MdpDb;

public class TransferCalculationRepository : ITransferCalculationRepository
{
    private readonly MdpDbContext _context;

    public TransferCalculationRepository(MdpDbContext context)
    {
        _context = context;
    }

    public async Task<Option<TransferCalculation>> Find(string businessGroup, string referenceNumber)
    {
        return await _context.Set<TransferCalculation>()
            .SingleOrDefaultAsync(j => j.BusinessGroup == businessGroup &&
                                       j.ReferenceNumber == referenceNumber);
    }

    public void Remove(TransferCalculation journey)
    {
        _context.Set<TransferCalculation>().Remove(journey);
    }

    public async Task Create(TransferCalculation journey)
    {
        await _context.Set<TransferCalculation>().AddAsync(journey);
    }
    
    public async Task CreateIfNotExists(TransferCalculation transferCalculation)
    {
        var existingTransferCalculation = await Find(transferCalculation.BusinessGroup, transferCalculation.ReferenceNumber);
        if (existingTransferCalculation.IsNone)
        {
            await Create(transferCalculation);
        }
    }
}