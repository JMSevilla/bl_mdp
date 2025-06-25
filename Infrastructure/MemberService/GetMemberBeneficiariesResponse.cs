using System.Collections.Generic;
using WTW.MdpService.Beneficiaries;

namespace WTW.MdpService.Infrastructure.MemberService;

public class GetMemberBeneficiariesResponse
{
    public List<BeneficiaryPersonResponse> People { get; set; }
    public List<BeneficiaryOrganizationResponse> Organizations { get; set; }
}
