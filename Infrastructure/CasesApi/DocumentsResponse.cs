using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace WTW.MdpService.Infrastructure.CasesApi;

public record DocumentsResponse
{
    [JsonPropertyName("bgroup")]
    public string BusinessGroup { get; init; }

    [JsonPropertyName("refno")]
    public string ReferenceNumber { get; init; }

    [JsonPropertyName("caseno")]
    public string CaseNumber { get; init; }

    [JsonPropertyName("casecode")]
    public string CaseCode { get; init; }

    [JsonPropertyName("documents")]
    public List<DocumentResponse> Documents { get; init; }
}