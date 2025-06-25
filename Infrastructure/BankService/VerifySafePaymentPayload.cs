namespace WTW.MdpService.Infrastructure.BankService;

public class VerifySafePaymentPayload
{
    public string BankCountryCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
}
