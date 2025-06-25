using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Common.Journeys;

namespace WTW.MdpService.Infrastructure.MdpDb;

public interface IJourneyDocumentsRepository
{
    Task<IList<UploadedDocument>> List(string businessGroup, string referenceNumber, string journeyType);
    Task<Option<UploadedDocument>> Find(string businessGroup, string referenceNumber, string fileUuid);
    Task Add(UploadedDocument document);
    void Remove(UploadedDocument document);
    void RemoveAll(IEnumerable<UploadedDocument> documents);
    Task<IList<UploadedDocument>> List(string businessGroup, string referenceNumber, List<string> journeyTypes);
}