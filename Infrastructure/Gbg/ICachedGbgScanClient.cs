using System.Threading.Tasks;

namespace WTW.MdpService.Infrastructure.Gbg;

public interface ICachedGbgScanClient
{
    Task<GbgAccessTokenResponse> CreateToken();
}