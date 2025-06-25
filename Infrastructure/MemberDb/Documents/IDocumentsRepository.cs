using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Domain.Members;
using WTW.Web.Pagination;
using WTW.Web.Sorting;
using WTW.Web.Specs;

namespace WTW.MdpService.Infrastructure.MemberDb.Documents;

public interface IDocumentsRepository
{
    void Add(Document document);
    Task AddRange(IList<Document> documents);
    Task<Option<Document>> FindByImageId(string referenceNumber, string businessGroup, int id);
    Task<Option<Document>> FindByDocumentId(string referenceNumber, string businessGroup, int id);
    Task<string[]> FindTypes(string referenceNumber, string businessGroup);
    Task<PaginatedList<Document>> List(Spec<Document> spec, Order<Document> order, Page page);
    Task<ICollection<Document>> List(string referenceNumber, string businessGroup, int[] ids);
    Task<int> NextId();
}