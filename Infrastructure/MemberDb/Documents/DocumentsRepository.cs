using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;
using WTW.Web.Pagination;
using WTW.Web.Sorting;
using WTW.Web.Specs;

namespace WTW.MdpService.Infrastructure.MemberDb.Documents;

public class DocumentsRepository : IDocumentsRepository
{
    private readonly MemberDbContext _context;

    public DocumentsRepository(MemberDbContext context)
    {
        _context = context;
    }

    public async Task<string[]> FindTypes(string referenceNumber, string businessGroup)
    {
        return await Query()
            .Where(x => x.ReferenceNumber == referenceNumber && x.BusinessGroup == businessGroup)
            .Select(x => x.Type)
            .Distinct()
            .ToArrayAsync();
    }

    public async Task<ICollection<Document>> List(string referenceNumber, string businessGroup, int[] ids)
    {
        return await Query()
            .Where(x => ids.Contains(x.Id) && x.BusinessGroup == businessGroup && x.ReferenceNumber == referenceNumber)
            .ToListAsync();
    }

    public async Task<PaginatedList<Document>> List(Spec<Document> spec, Order<Document> order, Page page)
    {
        return await order.Apply(Query())
            .Where(spec.ToExpression())
            .AsPaginated(page);
    }

    public async Task<Option<Document>> FindByImageId(string referenceNumber, string businessGroup, int id)
    {
        return await Query()
            .FirstOrDefaultAsync(x => x.ImageId == id && x.BusinessGroup == businessGroup && x.ReferenceNumber == referenceNumber);
    }

    public async Task<Option<Document>> FindByDocumentId(string referenceNumber, string businessGroup, int id)
    {
        return await Query()
            .FirstOrDefaultAsync(x => x.Id == id && x.BusinessGroup == businessGroup && x.ReferenceNumber == referenceNumber);
    }

    public async Task<int> NextId()
    {
        return (await _context.Database.ExecuteQuery(
            "SELECT WW_ECOMMS_METADATA_SEQ.nextval FROM DUAL",
            read => Convert.ToInt32(read["NEXTVAL"]))).First();
    }

    public void Add(Document document)
    {
        _context.Documents.Add(document);
    }

    private IQueryable<Document> Query()
    {
        return _context.Documents.AsQueryable();
    }

    public async Task AddRange(IList<Document> documents)
    {
        await _context.Documents.AddRangeAsync(documents);
    }
}