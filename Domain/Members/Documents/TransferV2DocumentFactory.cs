using System;

namespace WTW.MdpService.Domain.Members;

public class TransferV2DocumentFactory : IDocumentFactory
{
    public DocumentType DocumentType => DocumentType.TransferV2;
    public Document Create(string businessGroup, string referenceNumber, int id, int imageId, DateTimeOffset now, string caseNumber = null)
    {
        if (caseNumber == null)
            throw new ArgumentNullException(nameof(caseNumber));

        return new Document(
            businessGroup,
            referenceNumber,
            "Transfer Out Application",
            now,
            "Transfer Application",
            $"{businessGroup}-{referenceNumber}-{caseNumber}-{imageId}_mdp.pdf",
            id,
            imageId,
            "TRNAPP",
            "M");
    }
}