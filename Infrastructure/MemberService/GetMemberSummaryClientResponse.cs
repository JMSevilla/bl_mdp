using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.MemberService;

public class GetMemberSummaryClientResponse
{
#nullable enable
    public string? Bgroup { get; set; }
    public string? BgroupTranslation { get; set; }
    public string? Client { get; set; }
    public string? ClientTranslation { get; set; }
    public string? Category { get; set; }
    public string? CategoryTranslation { get; set; }
    public string? Scheme { get; set; }
    public string? SchemeType { get; set; }
    public string? SchemeTranslation { get; set; }
    public string? Status { get; set; }
    public string? StatusTranslation { get; set; }
    public string? SchemeCurrency { get; set; }
    public string? Forenames { get; set; }
    public string? Surname { get; set; }
    public int? RecordNumber { get; set; }
    public string? RecordType { get; set; }
    [JsonPropertyName("vulnerable")]
    public bool IsVulnerable { get; set; }
    public string CommunicationPreference { get; set; } = string.Empty;
    public bool HasEpaAccess { get; set; }
    [JsonPropertyName("epaCalcType")]
    public List<string> EpaCalcTypes { get; set; } = new List<string>();
    [JsonPropertyName("ipaCalcType")]
    public List<string> IpaCalcTypes { get; set; } = new List<string>();
#nullable disable
}