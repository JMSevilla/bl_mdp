using System;
using System.Collections.Generic;
using System.Linq;
using WTW.MdpService.BankAccounts;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Domain.Members;
using WTW.Web;

namespace WTW.MdpService.Journeys;

public class JourneyDataResponse
{
    public JourneyDataResponse(Member member, GenericJourneyData journeyData, IList<UploadedDocument> journeyDocuments)
    {
        ReferenceNumber = member.ReferenceNumber;
        BusinessGroup = member.BusinessGroup;
        FullName = (member.PersonalDetails.Forenames?.Trim() + " " + member.PersonalDetails.Surname?.Trim()).Trim();
        EmailAddress = member.Email().SingleOrDefault();
        Phone = member.FullMobilePhoneNumber().IsSome ? "+" + member.FullMobilePhoneNumber().SingleOrDefault() : null;
        Address = new AddressDataResponse(member.Address().SingleOrDefault());
        DateOfBirth = member.PersonalDetails.DateOfBirth;
        Journey = journeyData;
        BankAccount = member.EffectiveBankAccount().Match(x => BankAccountResponseV2.From(x), () => new BankAccountResponseV2());
        UploadedFilesNames = journeyDocuments.Where(x => x.DocumentType == null).Select(x => x.FileName);
        IdentityFilesNames = journeyDocuments.Where(x => x.DocumentType == MdpConstants.IdentityDocumentType).Select(x => x.FileName);
    }
    public string ReferenceNumber { get; set; }
    public string BusinessGroup { get; set; }
    public string FullName { get; set; }
    public string EmailAddress { get; set; }
    public string Phone { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public AddressDataResponse Address { get; set; }
    public GenericJourneyData Journey { get; set; }
    public BankAccountResponseV2 BankAccount { get; }
    public IEnumerable<string> UploadedFilesNames { get; }
    public IEnumerable<string> IdentityFilesNames { get; }
}

public class AddressDataResponse
{
    public AddressDataResponse(Address address)
    {
        Lines = address?.AddressLines();
        Country = address?.Country;
        CountryCode = address?.CountryCode;
        PostCode = address?.PostCode;
    }
    public IEnumerable<string> Lines { get; set; }
    public string Country { get; set; }
    public string CountryCode { get; set; }
    public string PostCode { get; set; }
}
