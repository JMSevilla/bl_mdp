using System.Threading.Tasks;
using Scriban;

namespace WTW.MdpService.Infrastructure.Templates.SingleAuth;

public class RegistrationEmailTemplate : IRegistrationEmailTemplate
{
    public async Task<string> RenderHtml(string htmlTemplate, string memberForenames, bool HasLinkedRecord)
    {
        return await Template.Parse(htmlTemplate).RenderAsync(
            new
            {
                memberForenames,
                HasLinkedRecord
            });
    }
}
