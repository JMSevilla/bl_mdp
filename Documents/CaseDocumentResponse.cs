using System;

namespace WTW.MdpService.Documents;

public record CaseDocumentResponse
{
    public string Tag { get; init; }
    public int? ImageId { get; init; }
    public string Narrative { get; init; }
    public DateTimeOffset? ReceivedDate { get; init; }
    public string Status { get; init; }
    public string Notes { get; init; }
}