using System.Collections.Generic;
using System.Linq;
using WTW.MdpService.Infrastructure.CasesApi;

namespace WTW.MdpService.Documents;

public record CaseDocumentsResponse
{
    public CaseDocumentsResponse(DocumentsResponse documentsResponse)
    {
        CaseNumber = documentsResponse.CaseNumber;
        CaseCode = documentsResponse.CaseCode;
        Documents = documentsResponse.Documents.Select(x => new CaseDocumentResponse
        {
            ImageId = x.ImageId,
            Tag = x.DocId,
            Narrative = x.Narrative,
            ReceivedDate = x.DateReceived,
            Status = x.Status,
            Notes = x.Notes
        }).ToList();
    }

    public string CaseNumber { get; init; }
    public string CaseCode { get; init; }
    public List<CaseDocumentResponse> Documents { get; init; }
}