using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.TokenService;

public record TokenServiceRequest
{
    public TokenServiceRequest(string grantType, string clientId, string clientSecret, IEnumerable<string> scopes)
    {
        GrantType = grantType;
        ClientId = clientId;
        ClientSecret = clientSecret;
        Scopes = scopes;
    }

    [JsonPropertyName("grant_type")]
    public string GrantType { get; }

    [JsonPropertyName("client_id")]
    public string ClientId { get; }

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; }

    [JsonPropertyName("scopes")]
    public IEnumerable<string> Scopes { get; }
}