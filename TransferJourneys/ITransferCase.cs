using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.TransferJourneys;

public interface ITransferCase
{
    Task<Either<Error, string>> Create(string businessGroup, string referenceNumber);
}
