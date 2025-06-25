namespace WTW.MdpService.Domain.Common.UploadedDocuments;

public class UploadedDocumentFactory : IUploadedDocumentFactory
{
    public UploadedDocument CreateOutgoing(string referenceNumber,
        string businessGroup,
        string fileName,
        string uuid,
        bool isEdoc,
        params string[] tags)
    {
        return new UploadedDocument(referenceNumber, businessGroup, null, null, fileName, uuid, DocumentSource.Outgoing, isEdoc, tags);
    }

    public UploadedDocument CreateIncoming(string referenceNumber,
        string businessGroup,
        string fileName,
        string uuid,
        bool isEdoc,
        params string[] tags)
    {
        return new UploadedDocument(referenceNumber, businessGroup, null, null, fileName, uuid, DocumentSource.Incoming, isEdoc, tags);
    }
}