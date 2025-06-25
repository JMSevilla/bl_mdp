using System.Linq;
using WTW.MdpService.Infrastructure.ApplyFinancials;

namespace WTW.MdpService.BankAccounts;

public record IbanBankAccountValidationResponse : BankAccountValidationResponse
{
    public string Iban { get; init; }
    public string Bic { get; init; }

    public static IbanBankAccountValidationResponse From(string accountName, AccountValidationResponse validation)
    {
        return new()
        {
            Name = accountName,
            Iban = validation.AccountNumber,
            Bic = validation.RecommendedBIC,
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