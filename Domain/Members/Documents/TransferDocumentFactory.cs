using System;

namespace WTW.MdpService.Domain.Members;

public class TransferDocumentFactory : IDocumentFactory
{
    public DocumentType DocumentType => DocumentType.Transfer;
    public Document Create(string businessGroup, string referenceNumber, int id, int imageId, DateTimeOffset now, string caseNumber = null)
    {
        return new Document(
            businessGroup,
            referenceNumber,
            "Transfer Quote",
            now,
            "Transfer Quote",
            $"{businessGroup}-{referenceNumber}_mdp.pdf",
            id,
            imageId,
            "TRNQU",
            "M");
    }
}