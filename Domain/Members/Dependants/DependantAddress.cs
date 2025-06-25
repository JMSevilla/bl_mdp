using System.Collections.Generic;
using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.Domain.Members.Dependants;

public class DependantAddress : ValueObject
{
    protected DependantAddress() { }

    private DependantAddress(string line1,
        string line2,
        string line3,
        string line4,
        string line5,
        string country,
        string postCode)
    {
        Line1 = line1;
        Line2 = line2;
        Line3 = line3;
        Line4 = line4;
        Line5 = line5;
        Country = country;
        PostCode = postCode;
    }

    public string Line1 { get; }
    public string Line2 { get; }
    public string Line3 { get; }
    public string Line4 { get; }
    public string Line5 { get; }
    public string Country { get; }
    public string PostCode { get; }

    public static Either<Error, DependantAddress> Create(string line1,
       string line2,
       string line3,
       string line4,
       string line5,
       string country,
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

        if (postCode?.Length > 8)
            return Error.New("PostCode must be up to 8 characters length.");

        return new DependantAddress(line1, line2, line3, line4, line5, country, postCode);
    }

    protected override IEnumerable<object> Parts()
    {
        yield return Line1;
        yield return Line2;
        yield return Line3;
        yield return Line4;
        yield return Line5;
        yield return Country;
        yield return PostCode;
    }

    public static DependantAddress Empty()
    {
        return new();
    }
}