using System;

namespace WTW.MdpService.Cases;

public class CaseListResponse
{
    public string CaseCode { get; set; }
    public string CaseStatus { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string CaseNumber { get; set; }
}
