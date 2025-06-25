using System.IO;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.Infrastructure.Aws;

public interface IAwsClient
{
    Task<Either<Error, MemoryStream>> File(string uri);
}