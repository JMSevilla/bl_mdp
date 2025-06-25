using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Edms;

public record LoginRequest
{
    [JsonPropertyName("username")]
    public string UserName { get; init; }

    [JsonPropertyName("password")]
    public string Password { get; init; }
}
