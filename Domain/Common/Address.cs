using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using WTW.Web;
using WTW.Web.Extensions;

namespace WTW.MdpService.Domain.Common;

public class Address : ValueObject
{
    protected Address() { }

    private Address(string streetAddress1,
        string streetAddress2,
        string streetAddress3,
        string streetAddress4,
        string streetAddress5,
        string country,
        string countryCode,
        string postCode)
    {
        StreetAddress1 = streetAddress1;
        StreetAddress2 = streetAddress2;
        StreetAddress3 = streetAddress3;
        StreetAddress4 = streetAddress4;
        StreetAddress5 = streetAddress5;
        Country = country;
        CountryCode = countryCode;
        PostCode = postCode;
    }

    public string StreetAddress1 { get; }
    public string StreetAddress2 { get; }
    public string StreetAddress3 { get; }
    public string StreetAddress4 { get; }
    public string StreetAddress5 { get; }
    public string Country { get; }
    public string CountryCode { get; }
    public string PostCode { get; }

    public static Either<Error, Address> Create(string streetAddress1,
        string streetAddress2,
        string streetAddress3,
        string streetAddress4,
        string streetAddress5,
        string country,
        string countryCode,
        string postCode)
    {
        if (streetAddress1?.Length > 50)
            return Error.New("StreetAddress1 must be up to 50 characters length.");

        if (streetAddress2?.Length > 50)
            return Error.New("StreetAddress2 must be up to 50 characters length.");

        if (streetAddress3?.Length > 50)
            return Error.New("StreetAddress3 must be up to 50 characters length.");

        if (streetAddress4?.Length > 50)
            return Error.New("StreetAddress4 must be up to 50 characters length.");

        if (streetAddress5?.Length > 50)
            return Error.New("StreetAddress5 must be up to 50 characters length.");

        if (country?.Length > 30)
            return Error.New("Country must be up to 30 characters length.");

        if (countryCode?.Length > 3)
            return Error.New("CountryCode must be up to 3 characters length.");

        if (postCode?.Length > 8)
            return Error.New("PostCode must be up to 8 characters length.");

        if (streetAddress1.HasHtmlTags() || streetAddress2.HasHtmlTags() ||
            streetAddress3.HasHtmlTags() || streetAddress4.HasHtmlTags() ||
            streetAddress5.HasHtmlTags() || country.HasHtmlTags() ||
            countryCode.HasHtmlTags() || postCode.HasHtmlTags())
        {
            return Error.New(MdpConstants.InputContainingHTMLTagError);
        }

        return new Address(streetAddress1, streetAddress2, streetAddress3, streetAddress4, streetAddress5, country, countryCode, postCode);
    }

    public static Address Empty()
    {
        return new Address();
    }

    public Address Clone()
    {
        return new Address(StreetAddress1, StreetAddress2, StreetAddress3, StreetAddress4, StreetAddress5, Country, CountryCode, PostCode);
    }

    public IEnumerable<string> AddressLines()
    {
        var lines = new List<string>();
        lines.AddRange(new[] { StreetAddress1, StreetAddress2, StreetAddress3, StreetAddress4, StreetAddress5 });
        return lines.Where(x => x != null);
    }

    protected override IEnumerable<object> Parts()
    {
        yield return StreetAddress1;
        yield return StreetAddress2;
        yield return StreetAddress3;
        yield return StreetAddress4;
        yield return StreetAddress5;
        yield return Country;
        yield return CountryCode;
        yield return PostCode;
    }
}