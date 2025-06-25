using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Retirement;

namespace WTW.MdpService.Infrastructure.Templates.RetirementApplication.Calculations;

public interface IRetirementApplicationCalculationTemplate
{
    Task<string> Render(string template, JsonElement optionsData, JsonElement options, CmsTokenInformationResponse cmsToken, Calculation calculation, IEnumerable<JsonElement> contentBlocks, (string AccessToken, string Env, string Bgroup) auth);
    Task<string> Render(string template, string selectedQuoteName, JsonElement summary, CmsTokenInformationResponse cmsToken, Calculation calculation, Member member, IEnumerable<JsonElement> contentBlocks, (string AccessToken, string Env, string Bgroup) auth);
}