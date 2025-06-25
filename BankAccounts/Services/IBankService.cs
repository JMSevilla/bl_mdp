using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using WTW.MdpService.Infrastructure.BankService;

namespace WTW.MdpService.BankAccounts.Services;

public interface IBankService
{
    Task<Either<Error, BankAccountResponseV2>> FetchBankAccount(string businessGroup, string referenceNumber);
    Task<Either<Error, ValidateBankAccountResponse>> ValidateBankAccount(string businessGroup, string referenceNumber, ValidateBankAccountRequest request);
    Task<Either<Error, ValidateBankAccountResponse>> ValidateAndSubmitBankAccount(string businessGroup, string referenceNumber, ValidateBankAccountRequest request);
    Task<Either<Error, IEnumerable<CountryCurrencyResponse>>> GetCountriesAndCurrencies();
}
