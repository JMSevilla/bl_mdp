namespace WTW.MdpService.BankAccounts;

public abstract record BankAccountValidationResponse
{
    public string Name { get; init; }
    public string BankName { get; init; }
    public string BranchName { get; init; }
    public string StreetAddress { get; init; }
    public string City { get; init; }
    public string Country { get; init; }
    public string CountryCode { get; init; }
    public string PostCode { get; init; }
}