using System.Collections.Generic;
using System.Text.Json.Serialization;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Content.V2;

public class AccessKey
{
    [JsonPropertyName("tenantUrl")]
    public string TenantUrl { get; set; }

    [JsonPropertyName("isCalculationSuccessful")]
    public bool? IsCalculationSuccessful { get; set; }

    [JsonPropertyName("hasAdditionalContributions")]
    public bool HasAdditionalContributions { get; set; }

    [JsonPropertyName("schemeType")]
    public string SchemeType { get; set; }

    [JsonPropertyName("memberStatus")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MemberStatus MemberStatus { get; set; }

    [JsonPropertyName("lifeStage")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MemberLifeStage LifeStage { get; set; }

    [JsonPropertyName("retirementApplicationStatus")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RetirementApplicationStatus RetirementApplicationStatus { get; set; }

    [JsonPropertyName("transferApplicationStatus")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransferApplicationStatus TransferApplicationStatus { get; set; }

    [JsonPropertyName("wordingFlags")]
    public IEnumerable<string> WordingFlags { get; set; }

    [JsonPropertyName("currentAge")]
    public string CurrentAge { get; set; }

    [JsonPropertyName("dbCalculationStatus")]
    public string DbCalculationStatus { get; set; }

    [JsonPropertyName("dcLifeStage")]
    public string DcLifeStage { get; set; }

    [JsonPropertyName("isWebChatEnabled")]
    public bool IsWebChatEnabled { get; set; }
}