using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.BankAccounts;

public record SubmitIbanBankAccountRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(3)]
    public string BankCountryCode { get; init; }

    [Required]
    [MinLength(4)]
    [MaxLength(34)]
    public string Iban { get; init; }

    [Required]
    [MinLength(1)]
    [MaxLength(40)]
    public string AccountName { get; init; }

    [Required]
    [MinLength(8)]
    [MaxLength(11)]
    public string Bic { get; init; }

    [MaxLength(11)]
    public string ClearingCode { get; init; }
}

public record ValidateIbanBankAccountRequest : SubmitIbanBankAccountRequest;