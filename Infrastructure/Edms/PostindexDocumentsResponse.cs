using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.Edms;

public record PostindexDocumentsResponse
{
    public List<PostindexDocumentResponse> Documents { get; init; }
}
