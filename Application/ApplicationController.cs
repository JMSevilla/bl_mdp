using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Application;

public class ApplicationController : ControllerBase
{
    private readonly IApplicationInitialization _applicationInitialization;
    private readonly IMemberRepository _memberRepository;
    private readonly ILogger<ApplicationController> _logger;

    public ApplicationController(IApplicationInitialization applicationInitialization, IMemberRepository memberRepository, ILogger<ApplicationController> logger)
    {
        _applicationInitialization = applicationInitialization;
        _memberRepository = memberRepository;
        _logger = logger;
    }

    [HttpPost("initialize")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> Initialize()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        _logger.LogInformation($"{nameof(Initialize)}, BusinessGroup: {businessGroup}, ReferenceNumber: {referenceNumber}");

        var member = await _memberRepository.FindMember(referenceNumber, businessGroup);
        if (member.IsNone)
        {
            _logger.LogError("Member for the given access token is not found. Business Group: {businessGroup}. Reference Number: {referenceNumber}.", businessGroup, referenceNumber);
            return NotFound(ApiError.FromMessage("Member for the given access token is not found."));
        }

        await _applicationInitialization.SetUpTransfer(member.Value());
        await _applicationInitialization.RemoveGenericJourneys(referenceNumber, businessGroup);
        await _applicationInitialization.UpdateGenericJourneysStatuses(referenceNumber, businessGroup);
        await _applicationInitialization.ClearSessionCache(referenceNumber, businessGroup);
        await _applicationInitialization.SetUpDcRetirement(member.Value());
        await _applicationInitialization.SetUpDbRetirement(member.Value());
        return NoContent();
    }
}