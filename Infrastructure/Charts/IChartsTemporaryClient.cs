using System.Text.Json;
using System.Threading.Tasks;

namespace WTW.MdpService.Infrastructure.Charts;

public interface IChartsTemporaryClient
{
    Task<JsonElement> GetChartJsonData(string tentantUrl);
}