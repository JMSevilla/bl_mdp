#nullable enable
using WTW;

namespace WTW.MdpService.IdentityVerification;

public class VerifyIdentityResponse
{
    public string? IdentityVerificationStatus { get; set; }
    public string? DocumentValidationStatus { get; set; }
}
#nullable disable
