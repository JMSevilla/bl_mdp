namespace WTW.MdpService.BankAccounts;

public record CountryCurrencyResponse
{
    public string CountryCode { get; init; }
    public string CountryName { get; init; }
    public string CurrencyCode { get; init; }
    public string CurrencyName { get; init; }
}
