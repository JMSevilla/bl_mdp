using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Calculations;

public class TypeResponse
{
    [JsonPropertyName("calctype")]
    public string Type { get; init; }
}
