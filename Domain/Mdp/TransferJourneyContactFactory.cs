using System;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using WTW.MdpService.Domain;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.Domain.Mdp;

public class TransferJourneyContactFactory : ITransferJourneyContactFactory
{
    public Either<Error, TransferJourneyContact> Create(string name, string advisorName, string companyName, Email email, Phone phone, string type, string schemeName, DateTimeOffset utcNow)
    {
        if (!string.IsNullOrWhiteSpace(companyName) && companyName.Count() > 50)
            return Error.New("Company name must be up to 50 characters length.");

        if (!string.IsNullOrWhiteSpace(name) && name.Count() > 50)
            return Error.New("Name must be up to 50 characters length.");

        return new TransferJourneyContact(name, advisorName, companyName, email, phone, type, schemeName, utcNow);
    }
}