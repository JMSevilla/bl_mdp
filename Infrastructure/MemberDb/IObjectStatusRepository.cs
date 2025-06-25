using System.Threading.Tasks;

namespace WTW.MdpService.Infrastructure.MemberDb;

public interface IObjectStatusRepository
{
    Task<string> FindTableShort(string businessGroup);
}