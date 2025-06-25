using System;
using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.CasesApi;

public record DocumentResponse
{
    [JsonPropertyName("docId")]
    public string DocId { get; init; }

    [JsonPropertyName("imageId")]
    public int? ImageId { get; init; }

    [JsonPropertyName("docNarrative")]
    public string Narrative { get; init; }

    [JsonPropertyName("dateReceived")]
    public DateTimeOffset? DateReceived { get; init; }

    [JsonPropertyName("docStatus")]
    public string Status { get; init; }

    [JsonPropertyName("docNotes")]
    public string Notes { get; init; }
}
