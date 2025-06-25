using System;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using WTW.MdpService.Domain.Common;

namespace WTW.MdpService.Domain.Mdp;

public class TransferJourneyContact
{
    protected TransferJourneyContact() { }

    public TransferJourneyContact(string name, string advisorName, string companyName, Email email, Phone phone, string type, string schemeName, DateTimeOffset utcNow)
    {
        Name = name;
        AdvisorName = advisorName;
        CompanyName = companyName;
        Email = email;
        Phone = phone;
        Type = type;
        CreatedAt = utcNow;
        Address = Address.Empty();
        SchemeName = schemeName;
    }

    public string Name { get; }
    public string AdvisorName { get; }
    public string CompanyName { get; }
    public Email Email { get; }
    public Phone Phone { get; }
    public Address Address { get; private set; }
    public string Type { get; }
    public string SchemeName { get; }
    public DateTimeOffset CreatedAt { get; set; }

    public void SubmitAddress(Address address)
    {
        Address = address;
    }
}