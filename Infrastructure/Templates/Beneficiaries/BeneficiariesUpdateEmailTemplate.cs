using System.Threading.Tasks;
using Scriban;

namespace WTW.MdpService.Infrastructure.Templates.Beneficiaries;

public static class BeneficiariesUpdateEmailTemplate
{
    public static async Task<string> RenderHtml(string htmlTemplate, string memberForenames)
    {
        return await Template.Parse(htmlTemplate).RenderAsync(
            new
            {
                memberForenames
            });
    }
}