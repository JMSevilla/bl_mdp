using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.DeloreanAuthentication;

public class OutboundSsoGenerateTokenClientResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
}
