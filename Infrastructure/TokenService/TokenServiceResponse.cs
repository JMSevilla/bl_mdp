using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.TokenService;
public record TokenServiceResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; init; }

    [JsonPropertyName("expires_in")]
    public int SecondsExpiresIn { get; init; }
}