using System.Threading.Tasks;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Application;

public interface IApplicationInitialization
{
    Task ClearSessionCache(string referenceNumber, string businessGroup);
    Task RemoveGenericJourneys(string referenceNumber, string businessGroup);
    Task UpdateGenericJourneysStatuses(string referenceNumber, string businessGroup);
    Task SetUpDcRetirement(Member member);
    Task SetUpDbRetirement(Member member);
    Task SetUpTransfer(Member member);
}