using WTW.MdpService.Domain.Members;
using WTW.Web.Extensions;

namespace WTW.MdpService.BankAccounts;

public record BankAccountResponse
{
    public string AccountName { get; init; }
    public string AccountNumber { get; init; }
    public string Iban { get; init; }
    public string SortCode { get; init; }
    public string SortCodeFormatted { get; init; }
    public string Bic { get; init; }
    public string ClearingCode { get; init; }
    public string BankName { get; init; }
    public string BankCity { get; init; }
    public string BankCountry { get; init; }
    public string BankCountryCode { get; init; }

    public static BankAccountResponse From(BankAccount account)
    {
        return new()
        {
            AccountName = account.AccountName,
            AccountNumber = account.AccountNumber,
            Iban = account.Iban,
            SortCode = account.Bank.SortCode,
            SortCodeFormatted = account.Bank.GetDashFormatedSortCode(),
            Bic = account.Bank.Bic,
            ClearingCode = account.Bank.ClearingCode,
            BankName = account.Bank.Name,
            BankCity = account.Bank.City,
            BankCountry = account.Bank.Country,
            BankCountryCode = account.Bank.CountryCode
        };
    }
}