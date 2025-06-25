using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Domain.Bereavement;

namespace WTW.MdpService.Infrastructure.BereavementDb;

public interface IBereavementJourneyRepository
{
    Task<Option<BereavementJourney>> Find(string businessGroup, Guid bereavmentReferenceNumber);
    Task<IEnumerable<BereavementJourney>> FindExpired(DateTimeOffset utcNow);
    Task<Option<BereavementJourney>> FindUnexpired(string businessGroup, Guid bereavmentReferenceNumber, DateTimeOffset utcNow);
    void Remove(BereavementJourney journey);
    void Remove(IEnumerable<BereavementJourney> journeys);
    Task Create(BereavementJourney journey);
}