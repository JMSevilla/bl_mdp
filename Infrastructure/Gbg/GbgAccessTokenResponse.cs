using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Gbg;

public record GbgAccessTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }
}
