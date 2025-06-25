using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using WTW.MdpService.BankAccounts.Services;
using WTW.Web;
using WTW.Web.Authorization;
using WTW.Web.Errors;

namespace WTW.MdpService.BankAccounts;

[ApiController]
[Route("api/v2/bank-accounts")]
public class BankAccountsV2Controller : ControllerBase
{
    private readonly ILogger<BankAccountsV2Controller> _logger;
    private readonly IBankService _bankService;

    public BankAccountsV2Controller(
        ILogger<BankAccountsV2Controller> logger,
        IBankService bankService)
    {
        _logger = logger;
        _bankService = bankService;
    }

    [HttpGet("current")]
    [ProducesResponseType(typeof(BankAccountResponseV2), 200)]
    [SwaggerResponse(404, "Not Found. Available error codes: member_bank_account_not_found.", typeof(ApiError))]
    public async Task<IActionResult> RetrieveBankAccountV2()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        var bankAccountResponse = await _bankService.FetchBankAccount(businessGroup, referenceNumber);

        return bankAccountResponse.Match<IActionResult>(
            success => Ok(success),
            error => NotFound(ApiError.From("Member does not have any bank account.", MdpConstants.BankErrorCodes.FailedBankAccountNotFoundErrorCode)));
    }

    [HttpPost("submit-uk-bank-account")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SubmitUkBankAccountV2([FromBody] ValidateBankAccountRequest request)
    {
        return await HandleBankAccountSubmission(request, nameof(SubmitUkBankAccountV2));
    }

    [HttpPost("submit-iban-bank-account")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SaveIbanBankAccountV2([FromBody] ValidateBankAccountRequest request)
    {
        return await HandleBankAccountSubmission(request, nameof(SaveIbanBankAccountV2));
    }

    [HttpPost("validate-iban-bank-account")]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(IbanBankAccountValidationResponse), 200)]
    public async Task<IActionResult> ValidateIbanBankAccountV2([FromBody] ValidateBankAccountRequest request)
    {
        return await HandleBankAccountValidation(request, nameof(ValidateIbanBankAccountV2));
    }

    [HttpPost("validate-uk-bank-account")]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(UkBankAccountValidationResponse), 200)]
    public async Task<IActionResult> ValidateUkBankAccountV2([FromBody] ValidateBankAccountRequest request)
    {
        return await HandleBankAccountValidation(request, nameof(ValidateUkBankAccountV2));
    }

    [HttpGet("countries-and-currencies")]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(List<CountryCurrencyResponse>), 200)]
    public async Task<IActionResult> GetCountryAndCurrencyList()
    {
        var result = await _bankService.GetCountriesAndCurrencies();

        return result.Match<IActionResult>(
            success => Ok(success),
            error => BadRequest(ApiError.From(error.Message, "failed_to_retrieve_countries_and_currencies")));
    }
    private async Task<IActionResult> HandleBankAccountSubmission(ValidateBankAccountRequest request, string actionName)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        _logger.LogInformation("{ActionName}, BusinessGroup: {BusinessGroup}, ReferenceNumber: {ReferenceNumber}",
            actionName, businessGroup, referenceNumber);

        var serviceResult = await _bankService.ValidateAndSubmitBankAccount(businessGroup, referenceNumber, request);

        return serviceResult.Match<IActionResult>(
            success => NoContent(),
            error =>
            {
                _logger.LogError("{ActionName}, BusinessGroup: {BusinessGroup}, ReferenceNumber: {ReferenceNumber} Error: {ErrorMessage}",
                    actionName, businessGroup, referenceNumber, error.Message);

                return error.Message switch
                {
                    MdpConstants.BankErrorCodes.FailedBankAccountNotFoundErrorCode
                        => NotFound(ApiError.From("Member does not have any bank account.", MdpConstants.BankErrorCodes.FailedBankAccountNotFoundErrorCode)),
                    MdpConstants.BankErrorCodes.FailedAccountNameValidationErrorCode
                        => BadRequest(ApiError.From("The account name does not match the one held on our records. Please try again.", MdpConstants.BankErrorCodes.FailedAccountNameValidationErrorCode)),
                    MdpConstants.BankErrorCodes.FailedExternalValidationErrorCode
                        => BadRequest(ApiError.From("The bank details provided are incorrect. Please try again.", MdpConstants.BankErrorCodes.FailedExternalValidationErrorCode)),
                    MdpConstants.BankErrorCodes.FailedBankAccountSubmitErrorCode
                        => BadRequest(ApiError.From("Error adding bank details.", MdpConstants.BankErrorCodes.FailedBankAccountSubmitErrorCode)),
                    _ => BadRequest(ApiError.From(error.Message, MdpConstants.BankErrorCodes.FailedExternalValidationErrorCode))
                };
            });
    }
    private async Task<IActionResult> HandleBankAccountValidation(ValidateBankAccountRequest request, string actionName)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        _logger.LogInformation("{ActionName}, BusinessGroup: {BusinessGroup}, ReferenceNumber: {ReferenceNumber}",
            actionName, businessGroup, referenceNumber);

        var validationResult = await _bankService.ValidateBankAccount(businessGroup, referenceNumber, request);

        return validationResult.Match<IActionResult>(
            success => Ok(success),
            error =>
            {
                _logger.LogError("{ActionName}, BusinessGroup: {BusinessGroup}, ReferenceNumber: {ReferenceNumber} Error: {ErrorMessage}",
                    actionName, businessGroup, referenceNumber, error.Message);

                return error.Message switch
                {
                    MdpConstants.BankErrorCodes.FailedBankAccountNotFoundErrorCode
                        => NotFound(ApiError.From("Member does not have any bank account.", MdpConstants.BankErrorCodes.FailedBankAccountNotFoundErrorCode)),
                    MdpConstants.BankErrorCodes.FailedAccountNameValidationErrorCode
                        => BadRequest(ApiError.From("The account name does not match the one held on our records. Please try again.", MdpConstants.BankErrorCodes.FailedAccountNameValidationErrorCode)),
                    MdpConstants.BankErrorCodes.FailedExternalValidationErrorCode
                        => BadRequest(ApiError.From("The bank details provided are incorrect. Please try again.", MdpConstants.BankErrorCodes.FailedExternalValidationErrorCode)),
                    _ => BadRequest(ApiError.From(error.Message, MdpConstants.BankErrorCodes.FailedExternalValidationErrorCode))
                };
            });
    }
}
