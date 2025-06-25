using System.Threading;
using System.Threading.Tasks;

namespace WTW.MdpService.Templates;

public interface ITemplateProvider
{
    Task<byte[]> GetTemplate(string templateName, string contentAccessKey, CancellationTokenSource cts);
}