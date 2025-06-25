using System.Collections.Generic;
using System.Text.Json.Serialization;
using WTW.MdpService.Domain.Common;

namespace WTW.MdpService.Infrastructure.Edms;

public record PostindexDocumentRequest
{
    public PostindexDocumentRequest(string uuid, List<string> types, bool isEdoc, bool isEpaOnly, DocumentSource? documentSource)
    {
        DocUuid = uuid;
        Types = types;
        IsEdoc = isEdoc;
        IsEpaOnly = isEpaOnly;
        DocumentSource = (documentSource ?? Domain.Common.DocumentSource.Incoming).ToDocSrcString();
    }

    [JsonPropertyName("docUuid")]
    public string DocUuid { get; init; }

    [JsonPropertyName("docTypes")]
    public List<string> Types { get; init; }

    [JsonPropertyName("eDoc")]
    public bool IsEdoc { get; init; }

    [JsonPropertyName("epaOnly")]
    public bool IsEpaOnly { get; init; }

    [JsonPropertyName("docSrc")]
    public string DocumentSource { get; init; }
}