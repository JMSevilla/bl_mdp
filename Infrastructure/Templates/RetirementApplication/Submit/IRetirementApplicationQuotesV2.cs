using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Retirement;

namespace WTW.MdpService.Infrastructure.Templates.RetirementApplication.Submit;

public interface IRetirementApplicationQuotesV2
{
    Task<RetirementApplicationSubmitionTemplateData> Create(RetirementJourney journey, IEnumerable<string> contentKeys, string contentAccessKey, CmsTokenInformationResponse cmsToken);
    Task<RetirementSummary> GetSummaryFigures(RetirementJourney journey, JsonElement retirementOptions);
}