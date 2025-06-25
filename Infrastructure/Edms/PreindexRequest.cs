using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Edms;

public record PreindexRequest
{
    [JsonPropertyName("bgroup")]
    public string BusinessGroup { get; init; }

    [JsonPropertyName("client")]
    public string Client { get; init; }

    [JsonPropertyName("refno")]
    public string ReferenceNumber { get; init; }

    [JsonPropertyName("fileblob")]
    public string FileBlob { get; init; }

    [JsonPropertyName("batchno")]
    public int? BatchNumber { get; init; }
}