using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.MemberService;

public class GetLinkedRecordClientResponse
{
    public List<LinkedMemberClientResponse> LinkedRecords { get; init; } = new List<LinkedMemberClientResponse>();
}
public class LinkedMemberClientResponse
{
    public string Bgroup { get; init; } = string.Empty;
    public string ReferenceNumber { get; init; } = string.Empty;
}