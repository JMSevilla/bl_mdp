using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Edms;

public record DocumentUploadResponse
{
    [JsonPropertyName("docUuid")]
    public string Uuid { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; }
}
