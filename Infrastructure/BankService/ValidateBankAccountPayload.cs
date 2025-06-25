#nullable enable

namespace WTW.MdpService.Infrastructure.BankService;

public class ValidateBankAccountPayload
{
    public string? AccountName { get; set; }
    public string? BankCountryCode { get; set; }
    public string? Iban { get; set; }
    public string? Bic { get; set; }
    public string? ClearingCode { get; set; }
    public string? AccountNumber { get; set; }
    public string? SortCode { get; set; }
    public string? AccountCurrency { get; set; }
}

#nullable disable
