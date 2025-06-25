#nullable enable

namespace WTW.MdpService.Infrastructure.BankService;

public class AddBankAccountPayload
{
    public string? ApplicationName { get; set; }
    public string? AccountName { get; set; }
    public string? AccountNumber { get; set; }
    public string? SortCode { get; set; }
    public string? ClearingCode { get; set; }
    public string? Iban { get; set; }
    public string? Bic { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountCurrency { get; set; }
    public string? BankCity { get; set; }
    public string? BankCountryCode { get; set; }
    public string? Country { get; set; }
}

#nullable disable
