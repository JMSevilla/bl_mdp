using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WTW.MdpService.Infrastructure.TokenService;
using WTW.Web;
using WTW.Web.Clients;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Infrastructure.DeloreanAuthentication;

public class DeloreanAuthenticationClient : IDeloreanAuthenticationClient
{
    private readonly ICachedTokenServiceClient _cachedTokenServiceClient;
    private readonly DeloreanAuthenticationOptions _options;
    private readonly HttpClient _client;
    private readonly ILogger<DeloreanAuthenticationClient> _logger;

    public DeloreanAuthenticationClient(ICachedTokenServiceClient cachedTokenServiceClient, HttpClient client, IOptionsSnapshot<DeloreanAuthenticationOptions> options,
                                        ILogger<DeloreanAuthenticationClient> logger)
    {
        _cachedTokenServiceClient = cachedTokenServiceClient;
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    private async Task<string> GetAccessToken()
    {
        return "Bearer " + (await _cachedTokenServiceClient.GetAccessToken()).AccessToken;
    }
    public async Task<Option<GetMemberAccessClientResponse>> GetMemberAccess(string application, Guid memberAuthGuid)
    {
        var token = await GetAccessToken();
        var result = await _client.GetOptionalJson<GetMemberAccessClientResponse>(
                string.Format(_options.GetMemberAbsolutePath, application, memberAuthGuid),
                (MdpConstants.AuthorizationHeaderName, $"{token}"));

        if (result.IsNone)
        {
            _logger.LogWarning("No memberAccess record found for {sub}", memberAuthGuid);
            return Option<GetMemberAccessClientResponse>.None;
        }

        return result.Value();
    }

    public async Task UpdateMember(string application, Guid memberGuid, Guid memberAuthGuid)
    {
        var token = await GetAccessToken();
        var requestMessage = new HttpRequestMessage(HttpMethod.Put,
       string.Format(_options.UpdateMemberAbsolutePath, application));
        requestMessage.Headers.Add(MdpConstants.AuthorizationHeaderName, token);
        requestMessage.Content = JsonContent.Create(new UpdateMemberClientRequest
        {
            MemberGuid = memberGuid,
            MemberAuthGuid = memberAuthGuid
        });

        var response = await _client.SendAsync(requestMessage);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Option<CheckEligibilityClientResponse>> CheckEligibility(string bgroup, string refNo, string application)
    {
        var token = await GetAccessToken();
        var result = await _client.GetOptionalJson<CheckEligibilityClientResponse>(
           string.Format(_options.CheckEligibilityAbsolutePath, application, bgroup, refNo),
           (MdpConstants.AuthorizationHeaderName, $"{token}"));

        if (result.IsNone)
        {
            _logger.LogWarning("Eligibility record not found for refno - {refno}", refNo);
            return Option<CheckEligibilityClientResponse>.None;
        }

        return result.Value();
    }

    public async Task RegisterRelatedMember(string bgroup, string refno, List<RegisterRelatedMemberDeloreanClientRequest> request)
    {
        var token = await GetAccessToken();

        await _client.PostJson<List<RegisterRelatedMemberDeloreanClientRequest>>(
       string.Format(_options.RegisterRelatedMemberAbsolutePath, bgroup, refno),
        request, (MdpConstants.AuthorizationHeaderName, $"{token}"));
    }

    public async Task<OutboundSsoGenerateTokenClientResponse> GenerateToken(OutboundSsoGenerateTokenClientRequest request)
    {
        var token = await GetAccessToken();
        var result = await _client.PostJson<OutboundSsoGenerateTokenClientRequest, OutboundSsoGenerateTokenClientResponse>(
                _options.GenerateTokenPath, request,
                (MdpConstants.AuthorizationHeaderName, $"{token}"));

        return result;
    }
}