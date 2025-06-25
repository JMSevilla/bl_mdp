using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Edms;

public record PreindexDocumentdResponse
{
    [JsonPropertyName("message")]
    public IList<Document> Documents { get; init; }

    [JsonPropertyName("imageid")]
    public int ImageId { get; init; }

    [JsonPropertyName("batchno")]
    public int BatchNumber { get; init; }

    public record Document
    {
        [JsonPropertyName("docUuid")]
        public string Id { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
        
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("imageId")]
        public string ImageId { get; set; }
    }
}
