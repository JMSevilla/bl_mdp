using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.BankAccounts;
public record SubmitUkBankAccountRequest
{
    [Required]
    [MinLength(8)]
    [MaxLength(8)]
    public string AccountNumber { get; init; }

    [Required]
    [MinLength(1)]
    [MaxLength(40)]
    public string AccountName { get; init; }

    [Required]
    [MinLength(6)]
    [MaxLength(6)]
    public string SortCode { get; init; }
}

public record ValidateUkBankAccountRequest : SubmitUkBankAccountRequest;