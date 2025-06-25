using System.Net.Http;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using WTW.Web.Clients;

namespace WTW.MdpService.Infrastructure.JobScheduler;

public class JobSchedulerClient : IJobSchedulerClient
{
    private readonly HttpClient _client;
    private readonly string _userName;
    private readonly string _password;

    public JobSchedulerClient(HttpClient client, string userName, string password)
    {
        _client = client;
        _userName = userName;
        _password = password;
    }

    public async Task<Either<Error, LoginResponse>> Login()
    {
        return await _client.PostJsonWithBasicAuth<LoginResponse, Error>(
            "/joc/api/security/login",
            _userName, _password);
    }

    public async Task<Either<Error, Unit>> Logout(string token)
    {
        return await _client.PostJson<Unit, Error>($"joc/api/security/logout", ("X-Access-Token", $"{token}"));
    }

    public async Task<Either<Error, OrderStatusResponse>> CheckOrderStatus(OrderRequest request, string token)
    {
        return await _client.PostJson<OrderRequest, OrderStatusResponse, Error>($"joc/api/orders/history", request, ("X-Access-Token", $"{token}"));
    }

    public async Task<Either<Error, Unit>> CreateOrder(OrderRequest request, string token)
    {
        return await _client.PostJson<OrderRequest, Unit, Error>($"joc/api/orders/add", request, ("X-Access-Token", $"{token}"));
    }
}