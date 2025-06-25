using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Templates;
using WTW.Web.Clients;

namespace WTW.MdpService.Infrastructure.Content;

public class ContentClient : IContentClient
{
    private readonly HttpClient _client;
    private readonly ILogger<ContentClient> _logger;

    public ContentClient(HttpClient client, ILogger<ContentClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<TemplateResponse> FindTemplate(string templateKey, string contentAccessKey)
    {
        return await _client.GetJson<TemplateResponse>($"/api/templates/{templateKey}/filtered?contentAccessKey={contentAccessKey}");
    }
    
    public async Task<TemplateResponse> FindTemplate(string templateKey, string contentAccessKey, string schemeCodeAndCategory)
    {
        return await _client.GetJson<TemplateResponse>($"/api/templates/{templateKey}/filtered?contentAccessKey={contentAccessKey}&schemeCodeAndCategory={schemeCodeAndCategory}");
    }

    public async Task<TemplateResponse> FindUnauthorizedTemplate(string templateKey, string tenantUrl)
    {
        return await _client.GetJson<TemplateResponse>($"/api/templates/{templateKey}/unauthorized?tenantUrl={tenantUrl}");
    }

    public async Task<JsonElement> FindSummaryBlocks(string key, string contentAccessKey)
    {
        return await _client.GetJson<JsonElement>($"/api/v2/content/authorized-option-summary?key={key}&contentAccessKey={contentAccessKey}&replaceLabels=true");
    }

    public async Task<IEnumerable<DataSummaryBlock>> FindDataSummaryBlocks(IEnumerable<string> keys, string contentAccessKey)
    {
        _logger.LogInformation("Initiating FindDataSummaryBlocks for contentAccessKey: {contentAccessKey}", contentAccessKey);

        var result = new List<DataSummaryBlock>();

        foreach (string key in keys)
        {
            try
            {
                var dataSummaryBlock = await _client.GetJson<JsonElement>($"/api/v2/content/authorized-data-summary?key={key}&contentAccessKey={contentAccessKey}&replaceLabels=true");
                result.Add( new DataSummaryBlock { Key = key, DataSummaryJsonElement = dataSummaryBlock });
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogWarning(httpEx, "HTTP error occurred while finding data summary blocks for contentAccessKey: {contentAccessKey}", contentAccessKey);
                continue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while finding data summary blocks for contentAccessKey: {contentAccessKey}", contentAccessKey);
                continue;
            }

        }

        return result;
    }

    public async Task<Option<JsonElement>> FindDataSummaryBlocks(string key, string contentAccessKey)
    {
        return await _client.GetOptionalJson<JsonElement>($"/api/v2/content/authorized-data-summary?key={key}&contentAccessKey={contentAccessKey}&replaceLabels=true");
    }
   
    public async Task<JsonElement> FindRetirementOptions(string contentAccessKey)
    {
        _logger.LogInformation("Initiating FindRetirementOptions for contentAccessKey: {contentAccessKey}", contentAccessKey);
        try
        {
            JsonElement? response = await _client.GetJson<JsonElement>($"/api/v2/content/authorized-option-list?contentAccessKey={contentAccessKey}&replaceLabels=true");

            if (response == null)
            {
                _logger.LogWarning("Received null response for contentAccessKey: {contentAccessKey}", contentAccessKey);
                throw new InvalidOperationException($"Null response received for contentAccessKey: {contentAccessKey}");
            }

            return response.Value;
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP error occurred while finding retirement options for contentAccessKey: {contentAccessKey}", contentAccessKey);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while finding retirement options for contentAccessKey: {contentAccessKey}", contentAccessKey);
            throw;
        }
    }

    public async Task<JsonElement> FindRetirementOptions(string selectedQuote, string contentAccessKey)
    {
        return await _client.GetJson<JsonElement>($"/api/v2/content/authorized-option-list?selectedQuoteName={selectedQuote}&contentAccessKey={contentAccessKey}&replaceLabels=true");
    }

    public async Task<JsonElement> FindTenant(string tenantUrl)
    {
        return await _client.GetJson<JsonElement>($"/api/v2/content/tenant-content?tenantUrl={tenantUrl}");
    }

    public async Task<TemplatesResponse> FindTemplates(string templateKey, string contentAccessKey)
    {
        return await _client.GetJson<TemplatesResponse>($"/api/templates/{templateKey}?contentAccessKey={contentAccessKey}");
    }

    public async Task<IEnumerable<JsonElement>> FindContentBlocks(IEnumerable<string> keys, string contentAccessKey)
    {
        _logger.LogInformation("Initiating FindContentBlocks for contentAccessKey: {contentAccessKey}", contentAccessKey);

        var result = new List<JsonElement>();

        foreach (string key in keys)
        {
            try
            {
                var contentBlock = await _client.GetJson<JsonElement>($"/api/v2/content/authorized-content-blocks?key={key}&contentAccessKey={contentAccessKey}&replaceLabels=true");
                result.Add(contentBlock);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogWarning(httpEx, "HTTP error occurred while finding content blocks for contentAccessKey: {contentAccessKey}", contentAccessKey);
                continue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while finding content blocks for contentAccessKey: {contentAccessKey}", contentAccessKey);
                continue;
            }
        }

        return result;
    }
}