using System.Collections.Generic;
using WTW.MdpService.SingleAuth.Services;

namespace WTW.MdpService.SingleAuth;

public record LinkedRecordsResponse
{
    public LinkedRecordsResponse()
    {

    }
    public LinkedRecordsResponse(LinkedRecordServiceResult result)
    {
        HasOutsideRecords = result.hasOutsideRecords;
        foreach (var item in result.Members)
        {
            Members.Add(new LinkedRecord
            {
                ReferenceNumber = item.ReferenceNumber,
                BusinessGroup = item.BusinessGroup,
                SchemeCode = item.SchemeCode,
                SchemeDescription = item.SchemeDescription,
                DateJoinedCompany = item.DateJoinedCompany,
                DateJoinedScheme = item.DateJoinedScheme,
                DateLeft = item.DateLeft,
                MemberStatus = item.MemberStatus,
                RecordNumber = item.RecordNumber,
                RecordType = item.RecordType
            });
        }
    }
    public bool HasOutsideRecords { get; init; }
    public List<LinkedRecord> Members { get; init; } = new();
}
public record class LinkedRecord
{
    public string ReferenceNumber { get; init; }
    public string BusinessGroup { get; init; }
    public string SchemeCode { get; init; }
    public string SchemeDescription { get; init; }
    public string DateJoinedCompany { get; init; }
    public string DateJoinedScheme { get; init; }
    public string DateLeft { get; init; }
    public string MemberStatus { get; init; }
    public int? RecordNumber { get; init; }
    public string RecordType { get; init; }
}
