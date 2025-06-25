using System;

namespace WTW.MdpService.Domain.Members;

public class RetirementQuoteWithoutCaseDocumantFactory : IDocumentFactory
{
    public DocumentType DocumentType => DocumentType.RetirementQuoteWithoutCase;

    public Document Create(string businessGroup, string referenceNumber, int id, int imageId, DateTimeOffset now, string caseNumber = null)
    {
        return new Document(
           businessGroup,
           referenceNumber,
           "Retirement quote",
           now,
           "Retirement quote",
           $"{businessGroup}-{referenceNumber}-{imageId}_mdp.pdf",
           id,
           imageId,
           "RETQU",
           "M");
    }
}