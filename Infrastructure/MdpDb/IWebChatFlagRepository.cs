using System.Collections.Generic;
using System.Threading.Tasks;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MdpDb;

public interface IWebChatFlagRepository
{
     Task<string> CheckBgroup(string bGroup);

    Task<string> CheckMemberCriteria(string bGroup, string schemeCode, string statusCode);
}
