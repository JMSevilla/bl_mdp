using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.CasesApi;

public record DocumentsErrorResponse
{
    [JsonPropertyName("error")]
    public string Error { get; init; }

    [JsonPropertyName("detail")]
    public string Detail { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; }
}