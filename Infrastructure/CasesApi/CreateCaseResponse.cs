using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.CasesApi;

public record CreateCaseResponse
{
    [JsonPropertyName("bgroup")]
    public string BusinessGroup { get; init; }

    [JsonPropertyName("batchno")]
    public int BatchNumber { get; init; }

    [JsonPropertyName("caseno")]
    public string CaseNumber { get; init; }

    [JsonPropertyName("error")]
    public object Error { get; init; }
}