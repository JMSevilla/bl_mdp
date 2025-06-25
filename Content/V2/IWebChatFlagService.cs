using System.Threading.Tasks;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Content.V2;

public interface IWebChatFlagService
{
    Task<bool> IsWebChatEnabledForBusinessGroup(string bGroup);

    Task<bool> IsWebChatEnabledForMemberCriteria(string bGroup, string schemeCode, string statusCode);
}
