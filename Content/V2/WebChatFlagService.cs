using System.Threading.Tasks;
using WTW.MdpService.Infrastructure.MdpDb;

namespace WTW.MdpService.Content.V2;

public class WebChatFlagService : IWebChatFlagService
{
    private readonly IWebChatFlagRepository _repo;

    public WebChatFlagService(IWebChatFlagRepository repo)
    {
        _repo = repo;
    }
    public async Task<bool> IsWebChatEnabledForBusinessGroup(string bGroup)
    {
        var value = await _repo.CheckBgroup(bGroup);

        if (value == null)
            return false;

        return value == "Y";
    }
    public async Task<bool> IsWebChatEnabledForMemberCriteria(string bGroup, string schemeCode, string statusCode)
    {
        var value = await _repo.CheckMemberCriteria(bGroup, schemeCode, statusCode);

        if (value == null)
            return false;

        return value == "1";
    }
}
