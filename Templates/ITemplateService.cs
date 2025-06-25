using System.Collections.Generic;
using System.Threading.Tasks;

namespace WTW.MdpService.Templates;

public interface ITemplateService
{
    Task<IList<byte[]>> DownloadTemplates(string contentAccessKey);
}