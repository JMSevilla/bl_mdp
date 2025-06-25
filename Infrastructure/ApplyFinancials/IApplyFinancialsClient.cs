using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.Infrastructure.ApplyFinancials;

public interface IApplyFinancialsClient
{
    Task<Either<Error, AccountValidationResponse>> ValidateIbanBankAccount(string iban, string bic, string countryCode);
    Task<Either<Error, AccountValidationResponse>> ValidateUkBankAccount(string accountNumber, string sortCode);
}