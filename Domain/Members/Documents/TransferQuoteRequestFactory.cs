using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WTW.MdpService.Domain.Members;

public class TransferQuoteRequestFactory : IDocumentFactory
{
    public DocumentType DocumentType => DocumentType.TransferQuoteRequest;
    public Document Create(string businessGroup, string referenceNumber, int id, int imageId, DateTimeOffset now, string caseNumber)
    {
        return new Document(
            businessGroup,
            referenceNumber,
            "Transfer Quote-Journey Summary(Assure)",
            now,
            "Assure Transfer Out Quote",
            $"{businessGroup}-{referenceNumber}-{caseNumber}-{imageId}_mdp.pdf",
            id,
            imageId,
            "TRNQU_SUM",
            "M");
    }
}