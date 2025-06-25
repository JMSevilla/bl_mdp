using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WTW.MdpService.SingleAuth.Services;
using WTW.Web;
using WTW.Web.Authentication;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.SingleAuth;

public class SingleAuthClaimsTransformation : IClaimsTransformation
{
    private readonly ILogger<SingleAuthClaimsTransformation> _logger;
    private readonly IOptions<SingleAuthAuthenticationOptions> _options;
    private readonly ISingleAuthService _singleAuthService;

    public SingleAuthClaimsTransformation(ILogger<SingleAuthClaimsTransformation> logger, IOptions<SingleAuthAuthenticationOptions> options,
                                          ISingleAuthService singleAuthService)
    {
        _logger = logger;
        _options = options;
        _singleAuthService = singleAuthService;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (_singleAuthService.IsAnonRequest())
        {
            return principal;
        }

        var identity = (ClaimsIdentity)principal.Identity;

        var scheme = principal.FindFirst(MdpConstants.AuthSchemeClaim)?.Value ?? "";

        if (_options.Value.Client.Exists(x => x.Name == scheme))
        {
            _logger.LogInformation("Transforming claims for scheme {scheme}", scheme);
            var subclaim = identity.FindFirst(ClaimTypes.NameIdentifier).Value;
            identity.AddClaim(new Claim(MdpConstants.MemberClaimNames.Sub, subclaim));

            if (_singleAuthService.IgnoreClaimTransformationCheck())
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, MdpConstants.MemberRole));
                return principal;
            }

            var tenantResult = _singleAuthService.GetCurrentTenant();

            if (tenantResult.IsLeft)
            {
                return principal;
            }

            var result = await _singleAuthService.CheckMemberAccess(Guid.Parse(subclaim), tenantResult.Right());
            if (result.IsLeft)
            {
                return principal;
            }

            identity.AddClaim(new Claim(ClaimTypes.Role, MdpConstants.MemberRole));
            identity.AddClaim(new Claim(MdpConstants.MemberClaimNames.BusinessGroup, result.Right().Bgroup));
            identity.AddClaim(new Claim(MdpConstants.MemberClaimNames.MainReferenceNumber, result.Right().MainReferenceNumber));
            identity.AddClaim(new Claim(MdpConstants.MemberClaimNames.MainBusinessGroup, result.Right().MainBgroup));
            identity.AddClaim(new Claim(MdpConstants.MemberClaimNames.ReferenceNumber, result.Right().ReferenceNumber));
            // Clear old NameIdentifier first
            if (identity.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                identity?.RemoveClaim(identity.FindFirst(ClaimTypes.NameIdentifier));
            }

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, result.Right().Bgroup + result.Right().ReferenceNumber));
            return principal;
        }
        return principal;
    }

}
