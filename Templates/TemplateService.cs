using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WTW.MdpService.Templates;

public class TemplateService : ITemplateService
{
    private readonly ITemplateProvider _templateProvider;

    public TemplateService(ITemplateProvider templateProvider)
    {
        _templateProvider = templateProvider;
    }

    public async Task<IList<byte[]>> DownloadTemplates(string contentAccessKey)
    {
        var templateNames = Enumerable.Range(1, 50).Select(i => $"transfer_pack_insert_{i}").ToList();
        var cts = new CancellationTokenSource();

        var tasks = templateNames.Select(templateName => _templateProvider.GetTemplate(templateName, contentAccessKey, cts)).ToList();

        byte[][] results = await Task.WhenAll(tasks);
        return results.Where(t => t != null).ToList();
    }
}
