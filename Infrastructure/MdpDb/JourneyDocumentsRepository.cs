using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Domain.Mdp.Calculations;

namespace WTW.MdpService.Infrastructure.MdpDb;

public class JourneyDocumentsRepository : IJourneyDocumentsRepository
{
    private readonly MdpDbContext _context;
    public JourneyDocumentsRepository(MdpDbContext context)
    {
        _context = context;
    }

    public async Task<IList<UploadedDocument>> List(string businessGroup, string referenceNumber, string journeyType)
    {
        return await _context.UploadedDocuments
            .Where(c =>
                c.BusinessGroup == businessGroup &&
                c.ReferenceNumber == referenceNumber &&
                c.JourneyType == journeyType)
            .ToListAsync();
    }

    public async Task<Option<UploadedDocument>> Find(string businessGroup, string referenceNumber, string fileUuid)
    {
        return await _context.UploadedDocuments
            .FirstOrDefaultAsync(c =>
            c.Uuid == fileUuid &&
            c.BusinessGroup == businessGroup &&
            c.ReferenceNumber == referenceNumber);
    }

    public async Task Add(UploadedDocument document)
    {
        await _context.UploadedDocuments.AddAsync(document);
    }

    public void Remove(UploadedDocument document)
    {
        _context.UploadedDocuments.Remove(document);
    }

    public void RemoveAll(IEnumerable<UploadedDocument> documents)
    {
        _context.UploadedDocuments.RemoveRange(documents);
    }

    public async Task<IList<UploadedDocument>> List(string businessGroup, string referenceNumber, List<string> journeyTypes)
    {
        return await _context.UploadedDocuments
           .Where(c =>
               c.BusinessGroup == businessGroup &&
               c.ReferenceNumber == referenceNumber)
           .Where(c => journeyTypes.Contains(c.JourneyType))
           .ToListAsync();
    }
}
