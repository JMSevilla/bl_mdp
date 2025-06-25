using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WTW.MdpService.Charts;
using WTW.MdpService.Infrastructure.Content;
using WTW.Web.Clients;
using WTW.Web.Serialization;

namespace WTW.MdpService.Infrastructure.Charts;

public class ChartsTemporaryClient : IChartsTemporaryClient
{
    private const string ChartsTemplateKey = "chart_data_example";
    private readonly IContentClient _contentClient;

    public ChartsTemporaryClient(IContentClient contentClient)
    {
        _contentClient = contentClient;
    }

    public async Task<JsonElement> GetChartJsonData(string tentantUrl)
    {
        var chartsTemplate = await _contentClient.FindUnauthorizedTemplate(ChartsTemplateKey, tentantUrl);
        var response = JsonSerializer.Deserialize<JsonElement>(chartsTemplate.HtmlBody, SerialiationBuilder.Options());
        return response;
    }
}
