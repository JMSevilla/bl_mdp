using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Infrastructure.BankService;

public class BankServiceOptions
{
    [Required]
    public string BaseUrl { get; set; }
    [Required]
    public string GetBankAccountPath { get; set; }
    [Required]
    public string PostBankAccountPath { get; set; }
    [Required]
    public string ValidateBankAccountPath { get; set; }
    [Required]
    public string VerifySafePaymentPath { get; set; }
}
