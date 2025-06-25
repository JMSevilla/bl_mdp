using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.Infrastructure.JobScheduler;

public interface IJobSchedulerClient
{
    Task<Either<Error, OrderStatusResponse>> CheckOrderStatus(OrderRequest request, string token);
    Task<Either<Error, Unit>> CreateOrder(OrderRequest request, string token);
    Task<Either<Error, LoginResponse>> Login();
    Task<Either<Error, Unit>> Logout(string token);
}