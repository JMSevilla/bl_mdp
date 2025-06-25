using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Edms;

public record AccessTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; }
}
