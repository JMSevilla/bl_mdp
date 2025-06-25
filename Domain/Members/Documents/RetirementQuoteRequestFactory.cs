using System;

namespace WTW.MdpService.Domain.Members;

public class RetirementQuoteRequestFactory : IDocumentFactory
{
    public DocumentType DocumentType => DocumentType.RetirementQuoteRequest;
    public Document Create(string businessGroup, string referenceNumber, int id, int imageId, DateTimeOffset now, string caseNumber)
    {
        return new Document(
            businessGroup,
            referenceNumber,
            "Retirement Quote-Journey Summary(Assure)",
            now,
            "Assure Retirement Quote",
            $"{businessGroup}-{referenceNumber}-{caseNumber}-{imageId}_mdp.pdf",
            id,
            imageId,
            "RETQU_SUM",
            "M");
    }
}