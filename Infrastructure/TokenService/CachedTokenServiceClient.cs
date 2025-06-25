using System;
using System.Threading.Tasks;
using WTW.Web.Caching;

namespace WTW.MdpService.Infrastructure.TokenService;

public class CachedTokenServiceClient : ICachedTokenServiceClient
{
    private readonly TokenServiceClient _client;
    private readonly ICache _cache;
    public CachedTokenServiceClient(TokenServiceClient client, ICache cache)
    {
        _client = client;
        _cache = cache;
    }

    public async Task<TokenServiceResponse> GetAccessToken()
    {
        var key = "token-service-get-token-scopes";
        return await _cache.Get<TokenServiceResponse>(key).ToAsync().IfNoneAsync(async () =>
        {
            var response = await _client.GetAccessToken();
            await _cache.Set(key, response, TimeSpan.FromSeconds(response.SecondsExpiresIn - 5));
            return response;
        });
    }
}