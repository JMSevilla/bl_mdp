using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Templates;

namespace WTW.MdpService.Infrastructure.Content;

public interface IContentClient
{
    Task<TemplateResponse> FindTemplate(string templateKey, string contentAccessKey);
    Task<TemplateResponse> FindTemplate(string templateKey, string contentAccessKey, string schemeCodeAndCategory);
    Task<TemplateResponse> FindUnauthorizedTemplate(string templateKey, string tenantUrl);
    Task<JsonElement> FindSummaryBlocks(string key, string contentAccessKey);
    Task<JsonElement> FindRetirementOptions(string selectedQuote, string contentAccessKey);
    Task<JsonElement> FindRetirementOptions(string contentAccessKey);
    Task<JsonElement> FindTenant(string tenantUrl);
    Task<TemplatesResponse> FindTemplates(string templateKey, string contentAccessKey);
    Task<Option<JsonElement>> FindDataSummaryBlocks(string key, string contentAccessKey);
    Task<IEnumerable<DataSummaryBlock>> FindDataSummaryBlocks(IEnumerable<string> keys, string contentAccessKey);
    Task<IEnumerable<JsonElement>> FindContentBlocks(IEnumerable<string> keys, string contentAccessKey);
}