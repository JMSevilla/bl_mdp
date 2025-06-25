using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb;

public class CalculationHistoryRepository : ICalculationHistoryRepository
{
    private readonly MemberDbContext _context;

    public CalculationHistoryRepository(MemberDbContext context)
    {
        _context = context;
    }

    public async Task<Option<CalculationHistory>> FindByEventTypeAndSeqNumber(string businessGroup, string referenceNumber, string @event, int seqNo)
    {
        return await _context.CalculationHistories
            .Where(x =>
                x.BusinessGroup == businessGroup &&
                x.ReferenceNumber == referenceNumber &&
                x.Event == @event &&
                x.SequenceNumber == seqNo)
            .FirstOrDefaultAsync();
    }

    public async Task<Option<CalculationHistory>> FindLatest(string businessGroup, string referenceNumber, int calcSystemHistorySeqno)
    {
        return await _context.CalculationHistories
            .Where(x => x.BusinessGroup == businessGroup &&
                    x.ReferenceNumber == referenceNumber &&
                    x.SequenceNumber == (calcSystemHistorySeqno > 0 ? calcSystemHistorySeqno : x.SequenceNumber))
            .OrderByDescending(x => x.SequenceNumber)
            .FirstOrDefaultAsync();
    }
}