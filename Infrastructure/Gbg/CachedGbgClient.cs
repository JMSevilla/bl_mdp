using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LanguageExt;
using WTW.Web.Caching;

namespace WTW.MdpService.Infrastructure.Gbg;

public class CachedGbgClient : ICachedGbgClient
{
    private readonly GbgClient _client;
    private readonly ICache _cache;
    private readonly int _expiresInMs;

    public CachedGbgClient(GbgClient client, ICache cache, int expiresInMs)
    {
        _client = client;
        _cache = cache;
        _expiresInMs = expiresInMs;
    }

    public TryAsync<Stream> GetDocuments(ICollection<Guid> ids)
    {
        return async () =>
        {
            var token = await CreateToken();
            return await _client.GetDocuments(ids, token.AccessToken);
        };
    }

    private async Task<GbgAccessTokenResponse> CreateToken()
    {
        return await _cache.Get<GbgAccessTokenResponse>("gbg_client_token").ToAsync().IfNoneAsync(async () =>
        {
            var newToken = await _client.CreateToken();
            await _cache.Set("gbg_client_token", newToken, TimeSpan.FromMilliseconds(_expiresInMs));
            return newToken;
        });
    }
}