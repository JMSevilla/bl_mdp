using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.TelephoneNoteService;

public record IntentContextResponse
{
    [JsonPropertyName("intent")]
    public string Intent { get; init; }

    [JsonPropertyName("ttl")]
    public string Ttl { get; init; }

    [JsonPropertyName("sessionId")]
    public string SessionId { get; init; }
} 