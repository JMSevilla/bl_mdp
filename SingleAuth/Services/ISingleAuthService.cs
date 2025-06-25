using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.SingleAuth.Services;
public interface ISingleAuthService
{
    Task<Either<Error, bool>> RegisterUser(string tenantUrl);
    Task<Either<Error, List<LinkedRecordServiceResultData>>> GetLoginDetails();
    Task<Either<Error, LinkedRecordServiceResult>> GetLinkedRecordTableData();
    Either<Error, Guid> GetSingleAuthClaim(ClaimsPrincipal principal, string claimName);
    Task<Either<Error, BgroupRefnoData>> CheckMemberAccess(Guid sub, string tenantBgroup);
    bool IgnoreClaimTransformationCheck();
    Either<Error, string> GetCurrentTenant();
    Task<List<(string bGroup, string RefNo)>> GetLinkedRecord(Guid sub, string TenantBgroup);
    Task<string> GetOutboundToken(int? recordNumber, bool hasMultipleRecords);
    bool IsAnonRequest();
}