using System;
using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.CasesApi;

public class CasesResponse
{
    [JsonPropertyName("creationDate")]
    public string CreationDate { get; set; }

    [JsonPropertyName("completionDate")]
    public string CompletionDate { get; set; }

    [JsonPropertyName("caseStatus")]
    public string CaseStatus { get; set; }

    [JsonPropertyName("caseno")]
    public string CaseNumber { get; set; }

    [JsonPropertyName("casecode")]
    public string CaseCode { get; set; }

    [JsonPropertyName("caseSource")]
    public string CaseSource { get; set; }
}