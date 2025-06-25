using System;

namespace WTW.MdpService.Domain.Members;

public class RetirementDocumentFactory : IDocumentFactory
{
    public DocumentType DocumentType => DocumentType.Retirement;
    public Document Create(string businessGroup, string referenceNumber, int id, int imageId, DateTimeOffset now, string caseNumber = null)
    {
        if (caseNumber == null)
            throw new ArgumentNullException(nameof(caseNumber));

        return new Document(
            businessGroup,
            referenceNumber,
            "MDP Retirement Application",
            now,
            "MDP Retirement Application",
            $"{businessGroup}-{referenceNumber}-{caseNumber}-{imageId}_mdp.pdf",
            id,
            imageId,
            "MDPRETAPP",
            "M");
    }
}