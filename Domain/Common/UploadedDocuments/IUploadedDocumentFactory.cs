namespace WTW.MdpService.Domain.Common.UploadedDocuments;

public interface IUploadedDocumentFactory
{
    UploadedDocument CreateIncoming(string referenceNumber, string businessGroup, string fileName, string uuid, bool isEdoc, params string[] tags);
    UploadedDocument CreateOutgoing(string referenceNumber, string businessGroup, string fileName, string uuid, bool isEdoc, params string[] tags);
}