using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WTW.Web.Clients;

namespace WTW.MdpService.Infrastructure.Gbg
{
    public class GbgAdminClient
    {
        private readonly HttpClient _client;
        private readonly string _userName;
        private readonly string _password;

        public GbgAdminClient(HttpClient client, string userName, string password)
        {
            _client = client;
            _userName = userName;
            _password = password;
        }

        public async Task<HttpStatusCode> DeleteJourneyPerson(string gbgId, string token)
        {
            var response = await _client.PostJson<string[]>(
            $"idscanenterprisesvc/entrymanagement/deletejourneyperson",
            new string[] { gbgId },
            ("Authorization", $"{token}"));
            response.EnsureSuccessStatusCode();

            return response.StatusCode;
        }


        public async Task<GbgAccessTokenResponse> CreateToken()
        {
            return (await _client.PostFromUrlEncoded<GbgAccessTokenResponse>(
                "idscanenterprisesvc/token",
                new Dictionary<string, string> {
                { "grant_type", "password" },
                { "area", "investigation" },
                { "username", _userName },
                { "password", _password }
                }));
        }
    }
}
