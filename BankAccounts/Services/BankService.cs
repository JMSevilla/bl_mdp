using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Infrastructure.BankService;
using WTW.MdpService.Infrastructure.IpaService;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.Web;
using WTW.Web.Extensions;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.BankAccounts.Services;

public class BankService : IBankService
{
    private readonly ILogger<BankService> _logger;
    private readonly IBankServiceClient _bankServiceClient;
    private readonly IIpaServiceClient _ipaServiceClient;
    private readonly IMemberRepository _memberRepository;

    public BankService(ILogger<BankService> logger,
        IBankServiceClient bankServiceClient,
        IIpaServiceClient ipaServiceClient,
        IMemberRepository memberRepository)
    {
        _logger = logger;
        _bankServiceClient = bankServiceClient;
        _ipaServiceClient = ipaServiceClient;
        _memberRepository = memberRepository;
    }

    public async Task<Either<Error, BankAccountResponseV2>> FetchBankAccount(string businessGroup, string referenceNumber)
    {
        _logger.LogInformation("{ActionName}, BusinessGroup: {BusinessGroup}, ReferenceNumber: {ReferenceNumber}",
            nameof(FetchBankAccount), businessGroup, referenceNumber);

        var bankAccountResponse = await _bankServiceClient.GetBankAccount(businessGroup, referenceNumber);
        if (bankAccountResponse.IsNone)
        {
            _logger.LogError("Bank details not found for BusinessGroup: {BusinessGroup}, ReferenceNumber: {ReferenceNumber}",
                businessGroup, referenceNumber);

            return Error.New(MdpConstants.BankErrorCodes.FailedBankAccountNotFoundErrorCode);
        }

        var bankAccount = new BankAccountResponseV2
        {
            AccountName = bankAccountResponse.Value().AccountName,
            AccountNumber = bankAccountResponse.Value().BankAccountNumber,
            Iban = bankAccountResponse.Value().IbanNumber,
            SortCode = bankAccountResponse.Value().LocalSortCode,
            SortCodeFormatted = GetDashFormatedSortCode(bankAccountResponse.Value().LocalSortCode),
            Bic = bankAccountResponse.Value().BicCode,
            ClearingCode = bankAccountResponse.Value().OverseasSortCode,
            BankName = bankAccountResponse.Value().BankName,
            BankCity = bankAccountResponse.Value().BankCity,
            BankCountry = bankAccountResponse.Value().Country,
            BankCountryCode = bankAccountResponse.Value().BankCountryCode,
            BankAccountCurrency = bankAccountResponse.Value().BankAccountCurrency
        };

        return bankAccount;
    }

    public async Task<Either<Error, ValidateBankAccountResponse>> ValidateBankAccount(string businessGroup, string referenceNumber, ValidateBankAccountRequest request)
    {
        var validationResult = await VerifyMemberSurnameMatch(referenceNumber, businessGroup, request.AccountName);
        if (validationResult.IsLeft)
        {
            return validationResult.Left();
        }

        var bankValidationResult = await ExternalBankAccountValidation(businessGroup, referenceNumber, request);
        if (bankValidationResult == null)
        {
            _logger.LogError("{ActionName}, ValidateBankAccount returned non success response for BusinessGroup: {BusinessGroup}, ReferenceNumber: {ReferenceNumber}",
                nameof(ValidateBankAccount), businessGroup, referenceNumber);
            return Error.New(MdpConstants.BankErrorCodes.FailedExternalValidationErrorCode);
        }

        if (request.BankCountryCode.Equals(MdpConstants.UkCountryCode, StringComparison.OrdinalIgnoreCase))
        {
            var safePaymentValidationResult = await VerifySafePayment(businessGroup, referenceNumber, request);
            if (safePaymentValidationResult == null
                || safePaymentValidationResult.Status.Equals(MdpConstants.BankValidationResult.Failed, StringComparison.OrdinalIgnoreCase)
                || safePaymentValidationResult.Status.Equals(MdpConstants.BankValidationResult.Caution, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError("{ActionName}, VerifySafePayment returned non success response for BusinessGroup: {BusinessGroup}, ReferenceNumber: {ReferenceNumber}",
                    nameof(ValidateBankAccount), businessGroup, referenceNumber);
                return Error.New(MdpConstants.BankErrorCodes.FailedExternalValidationErrorCode);
            }
        }

        return bankValidationResult;
    }

    public async Task<Either<Error, ValidateBankAccountResponse>> ValidateAndSubmitBankAccount(string businessGroup, string referenceNumber, ValidateBankAccountRequest request)
    {
        var bankValidationResult = await ValidateBankAccount(businessGroup, referenceNumber, request);
        if (bankValidationResult.IsLeft)
        {
            return bankValidationResult.Left();
        }

        var isAddBankAccountSuccessful = await AddBankAccount(businessGroup, referenceNumber, bankValidationResult.Right(), request);
        if (isAddBankAccountSuccessful.IsLeft)
        {
            return Error.New(MdpConstants.BankErrorCodes.FailedBankAccountSubmitErrorCode);
        }

        return bankValidationResult;
    }

    private static string GetDashFormatedSortCode(string sortCode)
    {
        if (string.IsNullOrEmpty(sortCode))
            return sortCode;

        var sortCodeChunks = Enumerable.Range(0, sortCode.Length / 2).Select(i => sortCode.Substring(i * 2, 2));
        return string.Join("-", sortCodeChunks);
    }

    private async Task<Either<Error, Unit>> VerifyMemberSurnameMatch(string referenceNumber, string businessGroup, string accountName)
    {
        var member = await _memberRepository.FindMember(referenceNumber, businessGroup);
        if (member.IsNone)
        {
            _logger.LogError("Member not found for reference number {referenceNumber} and business group {businessGroup}", referenceNumber, businessGroup);
            return Error.New(MdpConstants.BankErrorCodes.FailedBankAccountNotFoundErrorCode);
        }

        if (string.IsNullOrEmpty(member.Value().PersonalDetails.Surname) ||
            string.IsNullOrEmpty(accountName) ||
            !accountName.ContainsWithDiacriticsCheck(member.Value().PersonalDetails.Surname))
        {
            var errorMessage = "The account name does not match the one held on our records. Please try again.";
            _logger.LogError("{ActionName}, BusinessGroup: {businessGroup}, ReferenceNumber: {referenceNumber} Error: {errorMessage}",
                nameof(VerifyMemberSurnameMatch), businessGroup, referenceNumber, errorMessage);

            return Error.New(MdpConstants.BankErrorCodes.FailedAccountNameValidationErrorCode);
        }

        return Unit.Default;
    }
    private async Task<ValidateBankAccountResponse> ExternalBankAccountValidation(string bgroup, string refno, ValidateBankAccountRequest request)
    {
        _logger.LogInformation("{ActionName}, BusinessGroup: {BusinessGroup}, ReferenceNumber: {ReferenceNumber}",
            nameof(ExternalBankAccountValidation), bgroup, refno);

        return await _bankServiceClient.ValidateBankAccount(bgroup, refno, new ValidateBankAccountPayload
        {
            AccountName = request.AccountName,
            AccountNumber = request.AccountNumber,
            SortCode = request.SortCode,
            BankCountryCode = request.BankCountryCode,
            Iban = request.Iban,
            Bic = request.Bic,
            ClearingCode = request.ClearingCode,
            AccountCurrency = request.AccountCurrency
        });
    }

    private async Task<VerifySafePaymentResponse> VerifySafePayment(string businessGroup, string referenceNumber, ValidateBankAccountRequest request)
    {
        return await _bankServiceClient.VerifySafePayment(businessGroup, referenceNumber, new VerifySafePaymentPayload
        {
            BankCountryCode = request.BankCountryCode,
            FirstName = request.AccountName,
            AccountType = "Personal",
            NationalId = request.SortCode,
            AccountNumber = request.AccountNumber,
        });
    }
    private async Task<Either<Error, HttpStatusCode>> AddBankAccount(string bgroup, string refno, ValidateBankAccountResponse validationResponse,
        ValidateBankAccountRequest request)
    {
        _logger.LogInformation("{ActionName}, BusinessGroup: {BusinessGroup}, ReferenceNumber: {ReferenceNumber}",
            nameof(AddBankAccount), bgroup, refno);

        var addBankAccountResult = await _bankServiceClient.AddBankAccount(bgroup, refno, new AddBankAccountPayload
        {
            ApplicationName = "ASSURE",
            AccountName = validationResponse.Name,
            AccountNumber = validationResponse.AccountNumber,
            SortCode = validationResponse.SortCode,
            BankCountryCode = request.BankCountryCode,
            Iban = validationResponse.Iban,
            Bic = validationResponse.Bic,
            BankName = validationResponse.BankName,
            BankCity = validationResponse.City,
            BankAccountCurrency = request.AccountCurrency,
            ClearingCode = request.ClearingCode,
        });

        if (addBankAccountResult != HttpStatusCode.NoContent)
        {
            var error = $"Failed to Add Bank Details: {addBankAccountResult}";
            _logger.LogError("{ActionName}, Error: {error}", nameof(AddBankAccount), error);
            return Error.New(error);
        }

        return addBankAccountResult;
    }

    public async Task<Either<Error, IEnumerable<CountryCurrencyResponse>>> GetCountriesAndCurrencies()
    {
        var countriesTask = _ipaServiceClient.GetCountries();
        var currenciesTask = _ipaServiceClient.GetCurrencies();

        await Task.WhenAll(countriesTask, currenciesTask);

        var countriesOption = countriesTask.Result;
        if (countriesOption.IsNone)
        {
            _logger.LogError("Failed to fetch countries from IPA service.");
            return Error.New("Failed to fetch countries from IPA service.");
        }

        var currenciesOption = currenciesTask.Result;
        if (currenciesOption.IsNone)
        {
            _logger.LogError("Failed to fetch currencies from IPA service.");
            return Error.New("Failed to fetch currencies from IPA service.");
        }

        var countries = countriesOption.Value().Countries;

        var currencies = currenciesOption.Value().Currencies;

        var countryCurrencyResponses = from country in countries
                                       join currency in currencies on country.CountryCode equals currency.CountryCode
                                       select new CountryCurrencyResponse
                                       {
                                           CountryCode = country.CountryCode,
                                           CountryName = country.CountryName,
                                           CurrencyCode = currency.CurrencyCode,
                                           CurrencyName = currency.CurrencyName
                                       };

        return countryCurrencyResponses.OrderBy(x => x.CountryName).ToList();
    }
}
