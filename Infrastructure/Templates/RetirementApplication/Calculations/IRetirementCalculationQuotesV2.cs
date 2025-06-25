using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Infrastructure.Templates.Common.CmsContentModels;
using WTW.MdpService.Retirement;

namespace WTW.MdpService.Infrastructure.Templates.RetirementApplication.Calculations;

public interface IRetirementCalculationQuotesV2
{
    Task<(Dictionary<string, object>, List<SummaryBlock>)> Create(Calculation calculation, string selectedQuoteName, JsonElement summary);
    Task<(Dictionary<string, object>, List<SummaryBlock>)> Create(Calculation calculation, string selectedQuoteName, JsonElement summary, (string AccessToken, string Env, string Bgroup) auth);
    Option<OptionBlock> FilterOptionsByKey(JsonElement options, Calculation calculation, string key);
    List<ContentBlockItem> GetContentBlocks(IEnumerable<JsonElement> contentBlocks, CmsTokenInformationResponse cmsToken);
}