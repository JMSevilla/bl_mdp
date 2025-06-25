using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.MemberService;

public class GetMatchingMemberClientResponse
{
    public List<MemberReferenceDto> MemberList { get; set; } = new List<MemberReferenceDto> { };
    public class MemberReferenceDto
    {
        public string ReferenceNumber { get; set; }
    }
}
