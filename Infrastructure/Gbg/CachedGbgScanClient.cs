using System;
using System.Threading.Tasks;
using WTW.Web.Caching;

namespace WTW.MdpService.Infrastructure.Gbg;

public class CachedGbgScanClient : ICachedGbgScanClient
{
    private readonly GbgScanClient _client;
    private readonly ICache _cache;
    private readonly int _expiresInMs;

    public CachedGbgScanClient(GbgScanClient client, ICache cache, int expiresInMs)
    {
        _client = client;
        _cache = cache;
        _expiresInMs = expiresInMs;
    }

    public async Task<GbgAccessTokenResponse> CreateToken()
    {
        return await _cache.Get<GbgAccessTokenResponse>("gbg_scan_client_token").ToAsync().IfNoneAsync(async () =>
        {
            var newToken = await _client.CreateToken();
            await _cache.Set("gbg_scan_client_token", newToken, TimeSpan.FromMilliseconds(_expiresInMs));
            return newToken;
        });
    }
}