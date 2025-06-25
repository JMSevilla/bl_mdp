#nullable enable

namespace WTW.MdpService.Infrastructure.BankService;

public class ValidateBankAccountResponse
{
    public string? Name { get; set; }
    public string? BankName { get; set; }
    public string? BranchName { get; set; }
    public string? StreetAddress { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostCode { get; set; }
    public string? Iban { get; set; }
    public string? Bic { get; set; }
    public string? AccountNumber { get; set; }
    public string? SortCode { get; set; }
}
#nullable disable
