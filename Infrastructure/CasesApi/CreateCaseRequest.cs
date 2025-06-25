using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.CasesApi;

public record CreateCaseRequest
{
    [JsonPropertyName("bgroup")]
    public string BusinessGroup { get; init; }

    [JsonPropertyName("refno")]
    public string ReferenceNumber { get; init; }

    [JsonPropertyName("casecode")]
    public string CaseCode { get; init; }

    [JsonPropertyName("batchSource")]
    public string BatchSource { get; init; }

    [JsonPropertyName("batchDescription")]
    public string BatchDescription { get; init; }

    [JsonPropertyName("narrative")]
    public string Narrative { get; init; }

    [JsonPropertyName("notes")]
    public string Notes { get; init; }

    [JsonPropertyName("stickyNotes")]
    public string StickyNotes { get; init; }

    //these are from swagger doc but dont not exist in jira doc
    //public string ReasonCode { get; init; }
    //public string CaseEffectiveDate { get; init; }
    //public string Status { get; init; }
    //public bool CreateCase { get; init; }
    //public bool Complaint { get; init; }
    //public string Priority { get; init; }
    //public string Sensitive { get; init; }
    //public string CaseCreateDate { get; init; }
}