using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using WTW.Web.Clients;

namespace WTW.MdpService.Infrastructure.Gbg;

public class GbgScanClient
{
    private readonly HttpClient _client;
    private readonly string _userName;
    private readonly string _password;

    public GbgScanClient(HttpClient client, string userName, string password)
    {
        _client = client;
        _userName = userName;
        _password = password;
    }

    public async Task<GbgAccessTokenResponse> CreateToken()
    {
        return (await _client.PostFromUrlEncoded<GbgAccessTokenResponse>(
            "/idscanenterprisesvc/token",
            new Dictionary<string, string> {
                { "grant_type", "password" },
                { "area", "scanning" },
                { "username", _userName },
                { "password", _password }
            }));
    }
}