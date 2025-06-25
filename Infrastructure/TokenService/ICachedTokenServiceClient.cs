using System.Threading.Tasks;

namespace WTW.MdpService.Infrastructure.TokenService;

public interface ICachedTokenServiceClient
{
    Task<TokenServiceResponse> GetAccessToken();
}