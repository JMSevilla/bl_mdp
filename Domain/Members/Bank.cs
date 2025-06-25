using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.Domain.Members;

public class Bank : ValueObject
{
    protected Bank() { }

    private Bank(string sortCode,
        string bic,
        string clearingCode,
        string name,
        string city,
        string countryCode)
    {
        SortCode = sortCode;
        Bic = bic;
        ClearingCode = clearingCode;
        Name = Trim(name);
        City = city;
        CountryCode = countryCode;
        Country = countryCode;
    }

    public string SortCode { get; }
    public string Bic { get; }
    public string ClearingCode { get; }
    public string Name { get; }
    public string City { get; }
    public string Country { get; }
    public string CountryCode { get; }
    public string AccountCurrency { get; }

    public static Either<Error, Bank> CreateUkBank(string sortCode, string name, string city)
    {
        if (string.IsNullOrWhiteSpace(sortCode) || sortCode.Length != 6)
            return Error.New("Invalid Sort code: Must be 6 digit length.");

        return new Bank(sortCode, default, default, name, city, "GB");
    }

    public static Either<Error, Bank> CreateNonUkBank(string bic, string clearingCode, string name, string city, string countryCode)
    {
        if (string.IsNullOrWhiteSpace(bic) || (bic.Length != 8 && bic.Length != 11))
            return Error.New("Invalid BIC: Must be 8 or 11 digit length.");

        if (clearingCode != null && (clearingCode.Trim().Length < 1 || clearingCode.Length > 11))
            return Error.New("Invalid Clearing code: Must be between 1 and 11 digit length or null.");

        return new Bank(default, bic, clearingCode, name, city, countryCode);
    }

    public string GetDashFormatedSortCode()
    {
        if (string.IsNullOrEmpty(SortCode))
            return SortCode;

        var sortCodeChunks = Enumerable.Range(0, SortCode.Length / 2).Select(i => SortCode.Substring(i * 2, 2));
        return string.Join("-", sortCodeChunks);
    }

    protected override IEnumerable<object> Parts()
    {
        yield return Name;
        yield return City;
        yield return CountryCode;
        yield return SortCode;
        yield return Bic;
        yield return ClearingCode;
    }

    private static string Trim(string name)
    {
        var trimedName = name?.Trim();
        return trimedName != null && trimedName.Length > 35 ? trimedName.Substring(0, 35) : trimedName;
    }
}