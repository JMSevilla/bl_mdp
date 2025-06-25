using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.Domain.Members;

public class ContactCountry
{
    protected ContactCountry() { }

    public ContactCountry(long addressNumber, string country)
    {
        AddressNumber = addressNumber;
        Country = country;
    }

    public string ReferenceNumber { get; }
    public string BusinessGroup { get; }
    public long AddressNumber { get; }
    public string AddressCode { get; } = "GENERAL";
    public string PhoneType { get; } = "MOBPHON1";
    public string Country { get; }

    public static Either<Error, ContactCountry> Create(long addressNumber, string country)
    {
        if (string.IsNullOrWhiteSpace(country) || country.Length > 40)
            return Error.New("Invalid country: Must be between 1 and 80 characters length.");

        return new ContactCountry(addressNumber, country);
    }
}

