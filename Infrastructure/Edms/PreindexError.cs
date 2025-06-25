using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Edms;

public record PreindexError
{
    [JsonPropertyName("message")]
    public string Message { get; init; }
    public List<PostindexDocumentResponse> Documents { get; init; }
}
