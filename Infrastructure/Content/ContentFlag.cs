using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Content;

public class ContentFlag
{
    [JsonPropertyName("elementType")]
    public string ElementType { get; set; }

    [JsonPropertyName("value")]
    public bool Value { get; set; }
}
