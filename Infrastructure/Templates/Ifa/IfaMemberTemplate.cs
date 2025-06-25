using System.Threading.Tasks;
using Scriban;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.Templates.Ifa;

public static class IfaMemberTemplate
{
    public static async Task<string> Render(string template, PersonalDetails personalDetails)
    {
        return await Template.Parse(template).RenderAsync(
            new
            {
                MemberForenames = personalDetails.Forenames,
            });
        ;
    }
}