using System.Threading.Tasks;

namespace WTW.MdpService.Infrastructure.Templates.SingleAuth;

public interface IRegistrationEmailTemplate
{
    Task<string> RenderHtml(string htmlTemplate, string memberForenames, bool HasLinkedRecord);
}
