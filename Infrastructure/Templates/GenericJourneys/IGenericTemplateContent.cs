using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Threading.Tasks;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.Templates.Common.CmsContentModels;
using WTW.MdpService.Retirement;

namespace WTW.MdpService.Infrastructure.Templates.GenericJourneys;

public interface IGenericTemplateContent
{
    IEnumerable<ContentBlockItem> GetContentBlockItems(IEnumerable<JsonElement> contentBlocks, ExpandoObject obj, (string AccessToken, string Env, string Bgroup) auth);
    Task<List<SummaryBlock>>  GetDataSummaryBlocks(JsonElement summary, ExpandoObject obj, (string AccessToken, string Env, string Bgroup) auth);
    Task<List<DataSummaryItem>> GetDataSummaryBlocks(IEnumerable<DataSummaryBlock> summaries, ExpandoObject obj, (string AccessToken, string Env, string Bgroup) auth);
}