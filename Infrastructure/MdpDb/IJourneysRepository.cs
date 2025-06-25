using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Domain.Common.Journeys;

namespace WTW.MdpService.Infrastructure.MdpDb;

public interface IJourneysRepository
{
    Task Create(GenericJourney journey);
    Task<Option<GenericJourney>> Find(string businessGroup, string referenceNumber, string type);
    Task<ICollection<GenericJourney>> FindAllMarkedForRemoval(string businessGroup, string referenceNumber);
    void Remove(GenericJourney journey);
    void Remove(ICollection<GenericJourney> journeys);
    Task<ICollection<GenericJourney>> FindAll(string businessGroup, string referenceNumber);
    Task<ICollection<GenericJourney>> FindAllExpiredUnsubmitted(string businessGroup, string referenceNumber);
    Task<Option<GenericJourney>> FindUnexpired(string businessGroup, string referenceNumber, string type);
}