using System;
using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.DeloreanAuthentication;

public class GetMemberAccessClientResponse
{
    public List<GetMemberDataClient> Members { get; set; }
}
public class GetMemberDataClient
{
    public Guid? MemberGuid { get; set; }

    public string BusinessGroup { get; set; } = string.Empty;

    public string ReferenceNumber { get; set; } = string.Empty;

    public string Status { get; set; }
}