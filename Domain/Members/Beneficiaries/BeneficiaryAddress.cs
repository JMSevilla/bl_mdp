using System.Collections.Generic;
using LanguageExt;
using LanguageExt.Common;
using WTW.Web;
using WTW.Web.Extensions;

namespace WTW.MdpService.Domain.Members.Beneficiaries;

public class BeneficiaryAddress : ValueObject
{
    protected BeneficiaryAddress() { }

    private BeneficiaryAddress(string line1,
        string line2,
        string line3,
        string line4,
        string line5,
        string country,
        string countryCode,
        string postCode)
    {
        Line1 = line1;
        Line2 = line2;
        Line3 = line3;
        Line4 = line4;
        Line5 = line5;
        Country = country;
        CountryCode = countryCode;
        PostCode = postCode;
    }

    public string Line1 { get; }
    public string Line2 { get; }
    public string Line3 { get; }
    public string Line4 { get; }
    public string Line5 { get; }
    public string Country { get; }
    public string CountryCode { get; }
    public string PostCode { get; }

    public static Either<Error, BeneficiaryAddress> Create(string line1,
       string line2,
       string line3,
       string line4,
       string line5,
       string country,
       string countryCode,
       string postCode)
    {
        if (line1?.Length > 25)
            return Error.New("\'Address Line1\' must be up to 25 characters length.");

        if (line2?.Length > 25)
            return Error.New("\'Address Line2\' must be up to 25 characters length.");

        if (line3?.Length > 25)
            return Error.New("\'Address Line3\' must be up to 25 characters length.");

        if (line4?.Length > 25)
            return Error.New("\'Address Line4\' must be up to 25 characters length.");

        if (line5?.Length > 25)
            return Error.New("\'Address Line5\' must be up to 25 characters length.");

        if (country?.Length > 25)
            return Error.New("Country must be up to 25 characters length.");

        if (countryCode?.Length > 3)
            return Error.New("CountryCode must be up 3 characters length.");

        if (postCode?.Length > 8)
            return Error.New("PostCode must be up to 8 characters length.");

        if (line1.HasHtmlTags() || line2.HasHtmlTags() ||
            line3.HasHtmlTags() || line4.HasHtmlTags() ||
            line5.HasHtmlTags() || country.HasHtmlTags() ||
            countryCode.HasHtmlTags() || postCode.HasHtmlTags())
        {
            return Error.New(MdpConstants.InputContainingHTMLTagError);
        }

        return new BeneficiaryAddress(line1, line2, line3, line4, line5, country, countryCode, postCode);
    }

    protected override IEnumerable<object> Parts()
    {
        yield return Line1;
        yield return Line2;
        yield return Line3;
        yield return Line4;
        yield return Line5;
        yield return Country;
        yield return CountryCode;
        yield return PostCode;
    }

    public static BeneficiaryAddress Empty()
    {
        return new();
    }
}