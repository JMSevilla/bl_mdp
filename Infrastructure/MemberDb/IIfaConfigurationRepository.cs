using System.Threading.Tasks;
using LanguageExt;

namespace WTW.MdpService.Infrastructure.MemberDb;

public interface IIfaConfigurationRepository
{
    Task<Option<string>> FindEmail(string businessGroup, string calcType, string ifaName);
}