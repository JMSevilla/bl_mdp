using System.Threading.Tasks;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.Templates.TransferApplication;

public interface ITransferJourneySubmitEmailTemplate
{
    Task<string> RenderHtml(string htmlTemplate, Member member);
}