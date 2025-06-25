using System.Linq;
using WTW.MdpService.Infrastructure.ApplyFinancials;

namespace WTW.MdpService.BankAccounts;

public record UkBankAccountValidationResponse : BankAccountValidationResponse
{
    public string AccountNumber { get; init; }
    public string SortCode { get; init; }

    public static UkBankAccountValidationResponse From(string accountName, AccountValidationResponse validation)
    {
        return new()
        {
            Name = accountName,
            AccountNumber = validation.AccountNumber,
            SortCode = validation.NationalId,
            BankName = validation.BranchDetails.FirstOrDefault()?.BankName,
            BranchName = validation.BranchDetails.FirstOrDefault()?.Branch,
            StreetAddress = validation.BranchDetails.FirstOrDefault()?.Street,
            City = validation.BranchDetails.FirstOrDefault()?.City,
            Country = validation.BranchDetails.FirstOrDefault()?.Country,
            CountryCode = validation.CountryCode,
            PostCode = validation.BranchDetails.FirstOrDefault()?.PostZip,
        };
    }
}