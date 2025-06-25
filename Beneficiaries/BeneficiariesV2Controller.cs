using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.MdpService.Infrastructure.MemberService;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Beneficiaries;

[ApiController]
[Route("api/v2/members/current/beneficiaries")]
public class BeneficiariesV2Controller : ControllerBase
{
    private readonly ILogger<BeneficiariesV2Controller> _logger;
    private readonly IMemberServiceClient _memberServiceClient;
    private readonly IMemberRepository _memberRepository;

    public BeneficiariesV2Controller(ILogger<BeneficiariesV2Controller> logger,
                                     IMemberServiceClient memberServiceClient,
                                     IMemberRepository memberRepository)
    {
        _logger = logger;
        _memberServiceClient = memberServiceClient;
        _memberRepository = memberRepository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(BeneficiariesV2Response), 200)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> Beneficiaries([FromQuery] bool includeRevoked = false, [FromQuery] bool refreshCache = false)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        if (!await _memberRepository.ExistsMember(referenceNumber, businessGroup))
        {
            _logger.LogError("Member not found for reference number {referenceNumber} and business group {businessGroup}", referenceNumber, businessGroup);
            return NotFound(ApiError.NotFound());
        }

        var memberBeneficiariesResponse = await _memberServiceClient.GetBeneficiaries(businessGroup, referenceNumber, includeRevoked, refreshCache);
        if (memberBeneficiariesResponse.IsNone)
        {
            _logger.LogError("Failed to retrieve beneficiaries data for {businessGroup} {referenceNumber}", businessGroup, referenceNumber);
            return BadRequest(ApiError.FromMessage("Failed to retrieve beneficiaries data"));
        }

        var memberBeneficiaries = memberBeneficiariesResponse.Value();
        if ((memberBeneficiaries.People?.Any() != true) && (memberBeneficiaries.Organizations?.Any() != true))
        {
            _logger.LogInformation("No beneficiaries found for {businessGroup} {referenceNumber}", businessGroup, referenceNumber);
            return NoContent();
        }

        _logger.LogInformation("Beneficiaries returned successfully for {businessGroup} {referenceNumber}", businessGroup, referenceNumber);
        return Ok(memberBeneficiaries);
    }
}
