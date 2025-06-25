using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;

namespace WTW.MdpService.Infrastructure.DeloreanAuthentication;

public interface IDeloreanAuthenticationClient
{
    Task<Option<GetMemberAccessClientResponse>> GetMemberAccess(string application, Guid memberAuthGuid);
    Task UpdateMember(string application, Guid memberGuid, Guid memberAuthGuid);
    Task<Option<CheckEligibilityClientResponse>> CheckEligibility(string bgroup, string refNo, string application);
    Task RegisterRelatedMember(string bgroup, string refno, List<RegisterRelatedMemberDeloreanClientRequest> request);
    Task<OutboundSsoGenerateTokenClientResponse> GenerateToken(OutboundSsoGenerateTokenClientRequest request);
}
