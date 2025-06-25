using System.Collections.Generic;

namespace WTW.MdpService.Beneficiaries;

public class BeneficiariesV2Response
{
    public IEnumerable<BeneficiaryPersonResponse> People { get; set; }
    public IEnumerable<BeneficiaryOrganizationResponse> Organizations { get; set; }
}