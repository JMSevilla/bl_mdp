using System;
using System.Net.Http;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WTW.MdpService.Infrastructure.TokenService;
using WTW.Web.Clients;

namespace WTW.MdpService.Infrastructure.EpaService;

public class EpaServiceClient : IEpaServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ICachedTokenServiceClient _cachedTokenServiceClient;
    private readonly ILogger<EpaServiceClient> _logger;
    private readonly EpaServiceOptions _epaOptions;

    private const string BusinessGroupHeaderName = "BGROUP";

    public EpaServiceClient(HttpClient httpClient, ICachedTokenServiceClient cachedTokenServiceClient, ILogger<EpaServiceClient> logger,
                            IOptionsSnapshot<EpaServiceOptions> epaOptions)
    {
        _httpClient = httpClient;
        _cachedTokenServiceClient = cachedTokenServiceClient;
        _logger = logger;
        _epaOptions = epaOptions.Value;
    }

    public async Task<Option<WebRuleResultResponse>> GetWebRuleResult(string businessGroup, string referenceNumber, string userId, string ruleId, string schemeNo, Boolean cacheOptionFound)
    {
        _logger.LogInformation("GetWebRuleResult is called - bgroup: {businessGroup}, refno: {referenceNumber}, userId: {userId}, ruleId: {ruleId}, schemeNo: {schemeNo}", businessGroup, referenceNumber, userId, ruleId, schemeNo);

        try
        {
            return (await _httpClient.GetJson<WebRuleResultResponse, EpaServiceErrorResponse>(
                string.Format(_epaOptions.GetWebRuleAbsolutePath, businessGroup, referenceNumber, ruleId, schemeNo, userId) + (cacheOptionFound ? "&refreshCache=true" : ""),
                (BusinessGroupHeaderName, businessGroup),
                ("Authorization", $"{await GetAccessToken()}")))
                .Match(
                   x => x,
                   error =>
                   {
                       _logger.LogError("GetWebRuleResult - WebRule result endpoint for {businessGroup} {referenceNumber} " +
                                        "returned error: Message: {message}, Code: {code}", businessGroup, referenceNumber, error.Message, error.Code);
                       return Option<WebRuleResultResponse>.None;
                   }
                );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetWebRuleResult - Failed to retrieve WebRule result for {businessGroup} {referenceNumber}", businessGroup, referenceNumber);
            return Option<WebRuleResultResponse>.None;
        }
    }

    public async Task<Option<GetEpaUserClientResponse>> GetEpaUser(string businessGroup, string referenceNumber)
    {
        return (await _httpClient.GetOptionalJson<GetEpaUserClientResponse>(
        string.Format(_epaOptions.GetEpaUserAbsolutePath, businessGroup, referenceNumber),
         ("Authorization", $"{await GetAccessToken()}")));
    }

    private async Task<string> GetAccessToken()
    {
        return "Bearer " + (await _cachedTokenServiceClient.GetAccessToken()).AccessToken;
    }
}
