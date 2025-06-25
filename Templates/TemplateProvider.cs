using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Infrastructure.Content;

namespace WTW.MdpService.Templates;

public class TemplateProvider : ITemplateProvider
{
    private readonly IContentClient _contentClient;
    private readonly ILogger<TemplateProvider> _logger;

    public TemplateProvider(IContentClient contentClient, ILogger<TemplateProvider> logger)
    {
        _contentClient = contentClient;
        _logger = logger;
    }

    public async Task<byte[]> GetTemplate(string templateName, string contentAccessKey, CancellationTokenSource cts)
    {
        try
        {
            if (cts.IsCancellationRequested)
            {
                _logger.LogInformation($"CancellationTokenSource requested cancellation for template: {templateName}");
                return null;
            }

            var response = await _contentClient.FindTemplates(templateName, contentAccessKey);
            if (response == null)
            {
                _logger.LogInformation($"No file found for {templateName}");
                cts.Cancel();
                return null;
            }

            return response.Templates;
        }
        catch (Exception ex)
        {
            _logger.LogInformation("No more files found");
            cts.Cancel();
            return null;
        }
    }
}