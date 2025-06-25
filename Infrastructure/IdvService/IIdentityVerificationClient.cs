#nullable enable
using System.Threading.Tasks;
using WTW.MdpService.IdentityVerification;

namespace WTW.MdpService.Infrastructure.IdvService;

public interface IIdentityVerificationClient
{
    Task<VerifyIdentityResponse?> VerifyIdentity(string businessGroup, string referenceNumber, VerifyIdentityRequest payload);
    Task<UpdateIdentityResultResponse> SaveIdentityVerification(string businessGroup, string referenceNumber, SaveIdentityVerificationRequest payload);
}
#nullable disable
