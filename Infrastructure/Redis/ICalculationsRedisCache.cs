using System.Threading.Tasks;

namespace WTW.MdpService.Infrastructure.Redis;

public interface ICalculationsRedisCache
{
    Task Clear(string referenceNumber, string businessGroup);
    Task ClearRetirementDateAges(string referenceNumber, string businessGroup);
}