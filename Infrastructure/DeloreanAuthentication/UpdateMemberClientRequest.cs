using System;

namespace WTW.MdpService.Infrastructure.DeloreanAuthentication;

public class UpdateMemberClientRequest
{
    public Guid? MemberGuid { get; set; }
    public Guid? MemberAuthGuid { get; set; }
}
