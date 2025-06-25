using System.Threading.Tasks;

namespace WTW.MdpService.Infrastructure.MemberDb;

public interface ISystemRepository
{
    Task<long> NextAddressNumber();
    Task<long> NextAuthorizationNumber();
}