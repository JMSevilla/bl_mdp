using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Edms;

public record IndexResponse
{
    [JsonPropertyName("message")]
    public string Message { get; init; }
}
