using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Edms;

public record PostindexDocumentsRequest
{
    [JsonPropertyName("bgroup")]
    public string BusinessGroup { get; init; }

    [JsonPropertyName("client")]
    public string Client { get; init; }

    [JsonPropertyName("caseNo")]
    public string CaseNumber { get; init; }

    [JsonPropertyName("refno")]
    public string ReferenceNumber { get; init; }

    [JsonPropertyName("documents")]
    public IEnumerable<PostindexDocumentRequest> Documents { get; init; }
}