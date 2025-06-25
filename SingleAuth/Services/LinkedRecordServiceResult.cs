using System.Collections.Generic;

namespace WTW.MdpService.SingleAuth.Services;

public class LinkedRecordServiceResult
{
    public bool hasOutsideRecords { get; set; }
    public List<LinkedRecordServiceResultData> Members { get; set; } = new List<LinkedRecordServiceResultData>();

}
public class LinkedRecordServiceResultData
{
    public string BusinessGroup { get; set; }
    public string ReferenceNumber { get; set; }
    public string SchemeCode { get; set; }
    public string SchemeDescription { get; set; }
    public string DateJoinedCompany { get; set; }
    public string DateJoinedScheme { get; set; }
    public string DateLeft { get; set; }
    public string MemberStatus { get; set; }
    public int? RecordNumber { get; set; }
    public string RecordType { get; set; }
}
