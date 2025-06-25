using System;

namespace WTW.MdpService.Domain.Members;

public class DcRetirementDocumentFactory : IDocumentFactory
{
    public DocumentType DocumentType => DocumentType.DcRetirement;
    public Document Create(string businessGroup, string referenceNumber, int id, int imageId, DateTimeOffset now, string caseNumber)
    {
        return new Document(
            businessGroup,
            referenceNumber,
            "DC Retirement Application",
            now,
            "DC Retirement Application",
            $"{businessGroup}-{referenceNumber}-{caseNumber}-{imageId}_mdp.pdf",
            id,
            imageId,
            "MDPRETAPP",
            "M");
    }
}