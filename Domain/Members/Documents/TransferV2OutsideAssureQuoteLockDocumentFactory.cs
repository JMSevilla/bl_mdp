using System;

namespace WTW.MdpService.Domain.Members;

public class TransferV2OutsideAssureQuoteLockDocumentFactory: IDocumentFactory
{
    public DocumentType DocumentType => DocumentType.TransferV2OutsideAssureQuoteLock;
    public Document Create(string businessGroup, string referenceNumber, int id, int imageId, DateTimeOffset now, string caseNumber = null)
    {      
        return new Document(
            businessGroup,
            referenceNumber,
            "Transfer Quote",
            now,
            "Transfer Quote",
            $"{businessGroup}-{referenceNumber}-{imageId}_mdp.pdf",
            id,
            imageId,
            "TRNQU",
            "M");
    }
}