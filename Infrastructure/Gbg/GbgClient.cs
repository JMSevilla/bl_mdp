using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LanguageExt;
using WTW.Web.Clients;

namespace WTW.MdpService.Infrastructure.Gbg;

public class GbgClient
{
    private readonly HttpClient _client;
    private readonly string _userName;
    private readonly string _password;

    public GbgClient(HttpClient client, string userName, string password)
    {
        _client = client;
        _userName = userName;
        _password = password;
    }

    public async Task<Stream> GetDocuments(ICollection<Guid> ids, string token)
    {
        var response = await _client.Get(
            $"idscanenterprisesvc/reporting/ExportJourneyReports?evaluatedPersonEntryIds=[{string.Join(",", ids.Select(x => $"\"{x}\""))}]",
            ("Authorization", $"{token}"));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync();
    }

    public async Task<GbgAccessTokenResponse> CreateToken()
    {
        return (await _client.PostFromUrlEncoded<GbgAccessTokenResponse>(
            "/idscanenterprisesvc/token",
            new Dictionary<string, string> {
                { "grant_type", "password" },
                { "area", "investigation" },
                { "username", _userName },
                { "password", _password }
            }));
    }
}