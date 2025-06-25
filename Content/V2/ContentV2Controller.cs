using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.MdpService.Infrastructure.Redis;
using WTW.MdpService.Members;
using WTW.Web.Authorization;
using WTW.Web.Caching;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Content.V2;

[ApiController]
[Route("api/content")]
public class ContentV2Controller : ControllerBase
{
    private readonly ICalculationsRedisCache _calculationsRedisCache;
    private readonly ICache _cache;
    private readonly IContentService _contentService;
    private readonly IAccessKeyService _accessKeyService;
    private readonly ILogger<ContentV2Controller> _logger;
    private readonly IMemberRepository _memberRepository;

    public ContentV2Controller(
        IMemberRepository memberRepository,
        ICalculationsRedisCache calculationsRedisCache,
        ICache cache,
        IContentService contentService,
        ILogger<ContentV2Controller> logger,
        IAccessKeyService accessKeyService)
    {
        _memberRepository = memberRepository;
        _calculationsRedisCache = calculationsRedisCache;
        _cache = cache;
        _contentService = contentService;
        _logger = logger;
        _accessKeyService = accessKeyService;
    }
   
    [HttpGet("access-key")]
    [ProducesResponseType(typeof(ContentAccessKeyResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> AccessKey([FromQuery] ContentAccessKeyRequest request)
    {
        (string userId, string referenceNumber, string businessGroup, bool IsOpenAm) = HttpContext.User.UserWithAuthScheme();

        _logger.LogInformation("AccessKey is called - userId: {userId}, referenceNumber: {referenceNumber}, businessGroup: {businessGroup}", userId, referenceNumber, businessGroup);

        var member = await _memberRepository.FindMember(referenceNumber, businessGroup);
        var tenantContent = await _contentService.FindTenant(request.TenantUrl, businessGroup);
        if (!await _contentService.IsValidTenant(tenantContent, businessGroup) || member.IsNone)
        {
            member.IfNone(() => _logger.LogWarning("Member not found. ReferenceNumber: {referenceNumber}. BusinessGroup: {businessGroup}", referenceNumber, businessGroup));
            member.IfSome(_ => _logger.LogWarning("Invalid tenant url: {tenantUrl} for member: ReferenceNumber: {referenceNumber}. BusinessGroup: {businessGroup}", request.TenantUrl, referenceNumber, businessGroup));
            return NotFound(ApiError.NotFound());
        }

        await _cache.Remove(MembershipSummary.CacheKey(businessGroup, referenceNumber));
        await _calculationsRedisCache.Clear(referenceNumber, businessGroup);

        var accessKey = await _accessKeyService.CalculateKey(member.Value(), userId, request.TenantUrl,
            request.PreRetirementAgePeriodInYears, request.NewlyRetiredRangeInMonth, await _contentService.GetWebRuleWordingFlags(tenantContent),request.BasicMode, IsOpenAm);
        return Ok(ContentAccessKeyResponse.From(accessKey, member.Value().Scheme.Type,
                        member.Value().SchemeCode,
                        member.Value().Category));
    }

    [HttpGet("recalculate-access-key")]
    [ProducesResponseType(typeof(ContentAccessKeyResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> RecalculateAccessKey([FromQuery] ContentAccessKeyRequest request)
    {
        (string userId, string referenceNumber, string businessGroup, bool isOpenAm) = HttpContext.User.UserWithAuthScheme();

        _logger.LogInformation("RecalculateAccessKey is called - userId: {userId}, referenceNumber: {referenceNumber}, businessGroup: {businessGroup}", userId, referenceNumber, businessGroup);

        var member = await _memberRepository.FindMember(referenceNumber, businessGroup);
        var tenantContent = await _contentService.FindTenant(request.TenantUrl, businessGroup);
        if (!await _contentService.IsValidTenant(tenantContent, businessGroup) || member.IsNone)
        {
            member.IfNone(() => _logger.LogWarning("Member not found. ReferenceNumber: {referenceNumber}. BusinessGroup: {businessGroup}", referenceNumber, businessGroup));
            member.IfSome(_ => _logger.LogWarning("Invalid tenant url: {tenantUrl} for member: ReferenceNumber: {referenceNumber}. BusinessGroup: {businessGroup}", request.TenantUrl, referenceNumber, businessGroup));
            return NotFound(ApiError.NotFound());
        }

        var accessKey = await _accessKeyService.RecalculateKey(member.Value(), userId, request.TenantUrl, 
            request.PreRetirementAgePeriodInYears, request.NewlyRetiredRangeInMonth, await _contentService.GetWebRuleWordingFlags(tenantContent), request.BasicMode, isOpenAm);
        return Ok(ContentAccessKeyResponse.From(accessKey, member.Value().Scheme.Type,
                        member.Value().SchemeCode,
                        member.Value().Category));
    }
}