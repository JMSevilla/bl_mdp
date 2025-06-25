using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace WTW.MdpService.Infrastructure.Investment;

public class TargetSchemeMappingResponse
{
    [JsonPropertyName("targetContributionType")]
    public string ContributionType { get; set; }

    [JsonPropertyName("targetSchemeCode")]
    public string SchemeCode { get; set; }

    [JsonPropertyName("targetBusinessGroup")]
    public string Bgroup { get; set; }
}
