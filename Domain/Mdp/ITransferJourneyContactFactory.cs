using System;
using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.Domain.Mdp
{
    public interface ITransferJourneyContactFactory
    {
        Either<Error, TransferJourneyContact> Create(string name, string advisorName, string companyName, Email email, Phone phone, string type, string schemeName, DateTimeOffset utcNow);
    }
}