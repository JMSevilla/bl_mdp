using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.BankAccounts;

public record ValidateBankAccountRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(40)]
    public string AccountName { get; init; }

    [Required]
    [MinLength(2)]
    [MaxLength(2)]
    public string BankCountryCode { get; init; }

    public string Iban { get; init; }

    public string Bic { get; init; }

    public string ClearingCode { get; init; }

    public string AccountNumber { get; init; }

    public string SortCode { get; init; }

    [MinLength(3)]
    [MaxLength(3)]
    public string AccountCurrency { get; init; }
}
