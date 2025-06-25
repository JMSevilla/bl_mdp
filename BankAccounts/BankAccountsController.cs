using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.ApplyFinancials;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.BankAccounts;

[ApiController]
[Route("api/bank-accounts")]
public class BankAccountsController : ControllerBase
{
    private readonly IMemberRepository _memberRepository;
    private readonly IMemberDbUnitOfWork _uow;
    private readonly IApplyFinancialsClient _client;
    private readonly ILogger<BankAccountsController> _logger;

    public BankAccountsController(IMemberRepository memberRepository, IMemberDbUnitOfWork uow, IApplyFinancialsClient client, ILogger<BankAccountsController> logger)
    {
        _memberRepository = memberRepository;
        _uow = uow;
        _client = client;
        _logger = logger;
    }

    [HttpGet("current")]
    [ProducesResponseType(typeof(BankAccountResponse), 200)]
    [SwaggerResponse(404, "Not Found. Available error codes: member_bank_account_not_found.", typeof(ApiError))]
    public async Task<IActionResult> RetrieveBankAccount()
    {
        var businessGroup = HttpContext.User.User().BusinessGroup;
        var referenceNumber = HttpContext.User.User().ReferenceNumber;

        _logger.LogInformation($"{nameof(RetrieveBankAccount)} BusinessGroup: {businessGroup}, ReferenceNumber: {referenceNumber}");

        return (await _memberRepository.FindMember(referenceNumber, businessGroup))
            .Match<IActionResult>(m =>
            {
                var result = m.EffectiveBankAccount();
                if (result.IsNone)
                    return NotFound(ApiError.From("Member does not have any bank account.", "member_bank_account_not_found"));

                return Ok(BankAccountResponse.From(result.Value()));
            },
            () => NotFound(ApiError.NotFound()));
    }

    [HttpPost("submit-uk-bank-account")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SubmitUkBankAccount([FromBody] SubmitUkBankAccountRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        _logger.LogInformation($"{nameof(SubmitUkBankAccount)} BusinessGroup: {businessGroup}, ReferenceNumber: {referenceNumber}");

        return await _memberRepository.FindMember(referenceNumber, businessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async member =>
                {
                    var validationResult = await _client.ValidateUkBankAccount(request.AccountNumber, request.SortCode);
                    if (validationResult.IsLeft)
                        return BadRequest(ApiError.From("External bank details validation failed.", validationResult.Left().Message));

                    var bankBranch = validationResult.Right().BranchDetails.First();
                    var bankOrError = Bank.CreateUkBank(request.SortCode, bankBranch.BankName, bankBranch.City);
                    if (bankOrError.IsLeft)
                        return BadRequest(ApiError.FromMessage(bankOrError.Left().Message));

                    await _memberRepository.PopulateSessionDetails(businessGroup);
                    var result = member.TrySubmitUkBankAccount(request.AccountName,
                                                                request.AccountNumber,
                                                                DateTimeOffset.UtcNow,
                                                                bankOrError.Right());
                    if (result.IsLeft)
                        return BadRequest(ApiError.FromMessage(result.Left().Message));

                    await _uow.Commit();
                    await _memberRepository.DisableSysAudit(businessGroup);
                    return NoContent();
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpPost("submit-iban-bank-account")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SaveIbanBankAccount([FromBody] SubmitIbanBankAccountRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        _logger.LogInformation($"{nameof(SaveIbanBankAccount)} BusinessGroup: {businessGroup}, ReferenceNumber: {referenceNumber}");

        return await _memberRepository.FindMember(referenceNumber, businessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async member =>
                {
                    var validationResult = await _client.ValidateIbanBankAccount(request.Iban, request.Bic, request.BankCountryCode);
                    if (validationResult.IsLeft)
                        return BadRequest(ApiError.From("External bank details validation failed.", validationResult.Left().Message));

                    var bankBranch = validationResult.Right().BranchDetails.First();
                    var bankOrError = Bank.CreateNonUkBank(request.Bic, request.ClearingCode, bankBranch.BankName, bankBranch.City, request.BankCountryCode);
                    if (bankOrError.IsLeft)
                        return BadRequest(ApiError.FromMessage(bankOrError.Left().Message));

                    await _memberRepository.PopulateSessionDetails(businessGroup);
                    var result = member.TrySubmitIbanBankAccount(request.AccountName,
                                                                 request.Iban,
                                                                 DateTimeOffset.UtcNow,
                                                                 bankOrError.Right());

                    if (result.IsLeft)
                        return BadRequest(ApiError.FromMessage(result.Left().Message));

                    await _uow.Commit();
                    await _memberRepository.DisableSysAudit(businessGroup);
                    return NoContent();
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpPost("validate-iban-bank-account")]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(IbanBankAccountValidationResponse), 200)]
    public async Task<IActionResult> ValidateIbanBankAccount([FromBody] ValidateIbanBankAccountRequest request)
    {
        return (await _client.ValidateIbanBankAccount(request.Iban, request.Bic, request.BankCountryCode))
                    .Match<IActionResult>(
                        validation => Ok(IbanBankAccountValidationResponse.From(request.AccountName, validation)),
                        error => BadRequest(ApiError.From("External bank details validation failed.", error.Message)));
    }

    [HttpPost("validate-uk-bank-account")]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(UkBankAccountValidationResponse), 200)]
    public async Task<IActionResult> ValidateUkBankAccount([FromBody] ValidateUkBankAccountRequest request)
    {
        return (await _client.ValidateUkBankAccount(request.AccountNumber, request.SortCode))
                    .Match<IActionResult>(
                        validation => Ok(UkBankAccountValidationResponse.From(request.AccountName, validation)),
                        error => BadRequest(ApiError.From("External bank details validation failed.", error.Message)));
    }
}