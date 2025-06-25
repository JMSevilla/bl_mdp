using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Content;

public class ContentResponse
{
    [JsonPropertyName("elements")]
    public ContentElement Elements { get; set; }
}
