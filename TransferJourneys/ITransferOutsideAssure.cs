using System.Threading.Tasks;
using LanguageExt.Common;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.TransferJourneys;

public interface ITransferOutsideAssure
{
    Task CreateTransferForLockedQuote(string referenceNumber, string businessGroup);
    Task CreateTransferForEpa(Member member);
}