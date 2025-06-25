using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.Infrastructure.MdpDb;

public interface ITransferJourneyRepository
{
    Task<Option<TransferJourney>> Find(string businessGroup, string referenceNumber);
    void Remove(TransferJourney journey);
    Task Create(TransferJourney journey);
}