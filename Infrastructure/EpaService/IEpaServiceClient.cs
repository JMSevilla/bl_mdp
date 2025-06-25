using System;
using System.Threading.Tasks;
using LanguageExt;

namespace WTW.MdpService.Infrastructure.EpaService;

public interface IEpaServiceClient
{
    public Task<Option<WebRuleResultResponse>> GetWebRuleResult(string businessGroup, string referenceNumber, string userId, string ruleId, string schemeNo, Boolean cacheOptionFound);
    Task<Option<GetEpaUserClientResponse>> GetEpaUser(string businessGroup, string referenceNumber);
}
