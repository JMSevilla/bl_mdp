using System;
using System.Net;
using System.Threading.Tasks;
using LanguageExt;
using WTW.Web.Caching;

namespace WTW.MdpService.Infrastructure.Gbg;

public class CachedGbgAdminClient : ICachedGbgAdminClient
{
    private readonly GbgAdminClient _client;
    private readonly ICache _cache;
    private readonly int _expiresInMs;
    public CachedGbgAdminClient(
        GbgAdminClient client,
        ICache cache,
        int expiresInMs)
    {
        _client = client;
        _cache = cache;
        _expiresInMs = expiresInMs;
    }

    public TryAsync<HttpStatusCode> DeleteJourneyPerson(string gbgId)
    {
        return async () =>
        {
            var token = await CreateToken();
            return await _client.DeleteJourneyPerson(gbgId, token.AccessToken);
        };
    }

    private async Task<GbgAccessTokenResponse> CreateToken()
    {
        return await _cache.Get<GbgAccessTokenResponse>("gbg_admin_token").ToAsync().IfNoneAsync(async () =>
        {
            var newToken = await _client.CreateToken();
            await _cache.Set("gbg_admin_token", newToken, TimeSpan.FromMilliseconds(_expiresInMs));
            return newToken;
        });
    }
}