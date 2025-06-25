using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Edms;

public record PreindexResponse
{
    [JsonPropertyName("message")]
    public string Message { get; init; }

    [JsonPropertyName("imageid")]
    public int ImageId { get; init; }

    [JsonPropertyName("batchno")]
    public int BatchNumber { get; init; }
}
