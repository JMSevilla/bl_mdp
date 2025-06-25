using System.Collections.Generic;
using System.IO;
using System.Linq;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Compressions;

namespace WTW.MdpService.Documents;

public static class DocumentsPool
{
    public static IEnumerable<StreamFile> Aggregate(ICollection<(int Id, Stream Stream)> contents, ICollection<Document> documents)
    {
        return contents.Select(x => new StreamFile(documents.Single(y => y.Id == x.Id).FileName, x.Stream));
    }
}