using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Edms;

public record IndexRequest
{
    [JsonPropertyName("bgroup")]
    public string BusinessGroup { get; init; }

    [JsonPropertyName("refno")]
    public string ReferenceNumber { get; init; }

    [JsonPropertyName("caseno")]
    public string CaseNumber { get; init; }

    [JsonPropertyName("batchno")]
    public int BatchNumber { get; init; }

    [JsonPropertyName("casecode")]
    public string CaseCode { get; init; }

    [JsonPropertyName("docid")]
    public string DocumentId { get; init; }

    [JsonPropertyName("nonMemberDocument")]
    public bool NonMemberDocument { get; init; }
}