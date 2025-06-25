using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Tasks;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Retirement;

namespace WTW.MdpService.Infrastructure.Templates.RetirementApplication.Submit;

public interface IRetirementApplicationSubmissionTemplate
{
    Task<string> Render(string template, string contentAccessKey, CmsTokenInformationResponse cmsToken, RetirementJourney journey, Member member, string contentBlockKeys);
}