using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Domain;
using WTW.MdpService.Domain.Bereavement;

namespace WTW.MdpService.Infrastructure.BereavementDb;

public interface IBereavementContactConfirmationRepository
{
    Task Create(BereavementContactConfirmation token);
    Task<IEnumerable<BereavementContactConfirmation>> FindExpiredUnlocked(DateTimeOffset utcNow, int emailLockPeriodInMin);
    Task<Option<BereavementContactConfirmation>> FindLastEmailConfirmation(string businessGroup, Guid referenceNumber);
    Task<Option<BereavementContactConfirmation>> FindLocked(Email email);
    void Remove(BereavementContactConfirmation confirmation);
    void Remove(IEnumerable<BereavementContactConfirmation> confirmations);
}