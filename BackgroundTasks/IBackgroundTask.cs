using System.Threading;
using System.Threading.Tasks;

namespace WTW.MdpService.BackgroundTasks;

public interface IBackgroundTask
{
    Task Start(CancellationToken stoppingToken);
}