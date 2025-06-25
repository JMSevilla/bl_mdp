using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using WTW.MdpService.Infrastructure.IdvService;

namespace WTW.MdpService.IdentityVerification.Services;

public interface IIdvService
{
    Task<Either<Error, VerifyIdentityResponse>> VerifyIdentity(string businessGroup, string referenceNumber, string journeyType);
    Task<Either<Error, UpdateIdentityResultResponse>> SaveIdentityVerification(string businessGroup, string referenceNumber, string caseCode, string caseNumber);
}
