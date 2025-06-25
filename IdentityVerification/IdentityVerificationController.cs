using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WTW.MdpService.IdentityVerification.Services;
using WTW.MdpService.Infrastructure.Gbg;
using WTW.Web.Authorization;
using WTW.Web.Errors;

namespace WTW.MdpService.IdentityVerification;

[ApiController]
[Route("api/identity/verification")]
public class IdentityVerificationController : ControllerBase
{
    private readonly ICachedGbgScanClient _gbgClient;
    private readonly IIdvService _idvService;

    public IdentityVerificationController(ICachedGbgScanClient gbgClient, IIdvService idvService)
    {
        _gbgClient = gbgClient;
        _idvService = idvService;
    }

    [HttpPost("gbg/token")]
    [ProducesResponseType(typeof(GbgTokenResponse), 200)]
    public async Task<IActionResult> CreateToken()
    {
        var tokenResponse = await _gbgClient.CreateToken();
        return Ok(new GbgTokenResponse(tokenResponse.AccessToken));
    }

    [HttpPost("{journeyType}/verifyIdentity")]
    [ProducesResponseType(typeof(VerifyIdentityResponse), 200)]
    public async Task<IActionResult> VerifyIdentity(string journeyType)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        var verifyIdentityResponse = await _idvService.VerifyIdentity(businessGroup, referenceNumber, journeyType);

        return verifyIdentityResponse.Match<IActionResult>(
            r => Ok(r),
            l => BadRequest(ApiError.FromMessage(l.Message)));
    }
}