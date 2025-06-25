using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Edms;

public record DocumentUploadRequest
{
    [JsonPropertyName("bgroup")]
    public string BusinessGroup { get; init; }

    [JsonPropertyName("documentName")]
    public string DocumentName { get; init; }

    [JsonPropertyName("encodedDocument")]
    public string FileBlob { get; init; }

    [JsonPropertyName("batchno")]
    public int? BatchNumber { get; init; }
}