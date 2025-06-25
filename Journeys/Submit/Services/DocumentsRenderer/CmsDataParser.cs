using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using LanguageExt;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.MdpApi;

namespace WTW.MdpService.Journeys.Submit.Services.DocumentsRenderer;

public class CmsDataParser : ICmsDataParser
{
    private readonly PublicApiSetting _publicApiSetting;

    public CmsDataParser(PublicApiSetting publicApiSetting)
    {
        _publicApiSetting = publicApiSetting;
    }

    public IEnumerable<string> GetContentBlockKeys(TemplateResponse template)
    {
        return !string.IsNullOrEmpty(template.ContentBlockKeys) ? template.ContentBlockKeys.Split(";").Where(x => !string.IsNullOrEmpty(x)).ToList() : new List<string>();
    }

    public IEnumerable<string> GetDataSummaryKeys(TemplateResponse template)
    {
        return !string.IsNullOrEmpty(template.DataSummaryKeys) ? template.DataSummaryKeys.Split(";").Where(x => !string.IsNullOrEmpty(x)).ToList() : new List<string>();
    }

    public IEnumerable<Uri> GetContentBlockSourceUris(IEnumerable<JsonElement> contentBlocksContent)
    {
        var result = new List<Uri>();

        foreach (var contentBlockContent in contentBlocksContent)
        {
            if(contentBlockContent.ValueKind == JsonValueKind.Object)
            {
                var contentUris = GetDataSummaryBlockSourceUris(contentBlockContent);
                if (contentUris != null)
                    result.AddRange(contentUris);

                continue;
            }

            foreach (var contentBlock in contentBlockContent.EnumerateArray())
            {
                var uris = GetDataSummaryBlockSourceUris(contentBlock);
                if (uris != null)
                    result.AddRange(uris);
            }
        }
        
        return result.Distinct();
    }

    public IEnumerable<Uri> GetDataSummaryBlockSourceUris(JsonElement dataSummaryContent)
    {
        if (dataSummaryContent.TryGetProperty("elements", out var element)
            && element.TryGetProperty("dataSourceUrl", out var dataSourceUrl)
            && dataSourceUrl.TryGetProperty("value", out var value))
            return value.GetString()?.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => ParseUri(x));

        return Enumerable.Empty<Uri>();
    }

    private Uri ParseUri(string uri)
    {
        var newUriCreated = Uri.TryCreate(uri, UriKind.Absolute, out var result);
        
        if(result == null && Uri.TryCreate(_publicApiSetting.url, uri, out result))
            return result;

        if (newUriCreated && !(result.Scheme == Uri.UriSchemeHttps || result.Scheme == Uri.UriSchemeHttp))
        {
            var path = result.AbsolutePath.TrimStart('/');
            return new Uri(_publicApiSetting.url, path);
        }

        return result;
    }
}