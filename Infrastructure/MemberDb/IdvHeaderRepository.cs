using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb;

public class IdvHeaderRepository
{
    private readonly MemberDbContext _context;

    public IdvHeaderRepository(MemberDbContext context)
    {
        _context = context;
    }

    public async Task AddIdvHeader(IdvHeader idvHeader)
    {
        await _context.IdvHeaders.AddAsync(idvHeader);
    }

    public async Task<long> GetMaxSequenceNumber(string businessGroup, string referenceNumber, string schemeMember)
    {
        return await _context.IdvHeaders
            .Where(x => x.BusinessGroup == businessGroup && x.ReferenceNumber == referenceNumber && x.SchemeMember == schemeMember)
            .MaxAsync(x => (long?)x.SequenceNumber) ?? 0;
    }
}