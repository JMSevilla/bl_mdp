using System;

namespace WTW.MdpService.Domain.Members;

public interface IDocumentFactory
{
    DocumentType DocumentType { get; }
    Document Create(string businessGroup, string referenceNumber, int id, int imageId, DateTimeOffset now, string caseNumber = null);
}
