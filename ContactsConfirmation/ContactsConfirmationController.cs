using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.Db;
using WTW.MdpService.Infrastructure.EmailConfirmation;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.Web.Attributes;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;
using WTW.Web.Templates;

namespace WTW.MdpService.ContactsConfirmation;

[ApiController]
public class ContactsConfirmationController : ControllerBase
{
    private readonly ContactConfirmationRepository _contactConfirmationRepository;
    private readonly ContactsConfirmationConfiguration _contactsConfirmationConfiguration;
    private readonly MemberRepository _memberRepository;
    private readonly ISystemRepository _systemRepository;
    private readonly IObjectStatusRepository _objectStatusRepository;
    private readonly EmailConfirmationSmtpClient _emailConfirmationSmtpClient;
    private readonly MdpUnitOfWork _mdpUnitOfWork;
    private readonly MemberDbUnitOfWork _memberDbUnitOfWork;
    private readonly IMessageClient _messageClient;
    private readonly ContentClient _contentClient;
    private readonly ITenantRepository _tenantRepository;
    private readonly OtpSettings _otpSettings;
    private readonly ILogger<ContactsConfirmationController> _logger;

    public ContactsConfirmationController(ContactConfirmationRepository contactConfirmationRepository,
        ContactsConfirmationConfiguration contactsConfirmationConfiguration,
        MemberRepository memberRepository,
        ISystemRepository systemRepository,
        IObjectStatusRepository objectStatusRepository,
        EmailConfirmationSmtpClient emailConfirmationSmtpClient,
        MdpUnitOfWork mdpUnitOfWork,
        MemberDbUnitOfWork memberDbUnitOfWork,
        IMessageClient messageClient,
        ContentClient contentClient,
        ITenantRepository tenantRepository,
        OtpSettings otpSettings,
        ILogger<ContactsConfirmationController> logger
        )
    {
        _contactConfirmationRepository = contactConfirmationRepository;
        _contactsConfirmationConfiguration = contactsConfirmationConfiguration;
        _memberRepository = memberRepository;
        _systemRepository = systemRepository;
        _objectStatusRepository = objectStatusRepository;
        _emailConfirmationSmtpClient = emailConfirmationSmtpClient;
        _mdpUnitOfWork = mdpUnitOfWork;
        _memberDbUnitOfWork = memberDbUnitOfWork;
        _messageClient = messageClient;
        _contentClient = contentClient;
        _tenantRepository = tenantRepository;
        _otpSettings = otpSettings;
        _logger = logger;
    }

    [HttpPost]
    [Route("api/v2/members/contacts/confirmation/email/send")]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ContactConfirmationResponse), 200)]
    public async Task<IActionResult> SendConfirmationEmailV2([FromBody] SendEmailConfirmationV2Request request)
    {
        var utcNow = DateTimeOffset.UtcNow;

        var member = await _memberRepository.FindMember(HttpContext.User.User().ReferenceNumber, HttpContext.User.User().BusinessGroup);
        if (member.IsNone)
            return BadRequest(ApiError.FromMessage("Member was not found."));

        var emailOrError = Email.Create(request.EmailAddress);
        if (emailOrError.IsLeft)
            return BadRequest(ApiError.FromMessage(emailOrError.Left().Message));

        var confirmation = ContactConfirmation.CreateForEmail(
                       HttpContext.User.User().BusinessGroup,
                       HttpContext.User.User().ReferenceNumber,
                       RandomNumber.Get(),
                       emailOrError.Right(),
                       utcNow.AddMinutes(_contactsConfirmationConfiguration.EmailTokenExpiresInMin),
                       utcNow,
                       _contactsConfirmationConfiguration.MaxEmailConfirmationAttemptCount);

        var template = await _contentClient.FindTemplate("email_confirmation_message", request.ContentAccessKey, $"{member.Value().SchemeCode}-{member.Value().Category}");
        var htmlBody = new Template(template.HtmlBody).Apply(new Dictionary<string, string>()
        {
            ["security_token"] = confirmation.Token
        });
        await _contactConfirmationRepository.Create(confirmation);
        await _mdpUnitOfWork.Commit();

        try
        {
            await _emailConfirmationSmtpClient.Send(confirmation.Contact, template.EmailFrom, htmlBody, template.EmailSubject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email");
            return BadRequest(ApiError.FromMessage(ex.Message));
        }

        return Ok(ContactConfirmationResponse.From(confirmation.ExpiresAt));
    }

    [HttpPost]
    [Route("api/members/contacts/confirmation/email/confirm")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [TypeFilter(typeof(ResponseHashFilterAttribute))]
    public async Task<IActionResult> ConfirmEmail(EmailConfirmationRequest request)
    {
        return await _contactConfirmationRepository.FindLastEmailConfirmation(
               HttpContext.User.User().BusinessGroup,
               HttpContext.User.User().ReferenceNumber)
           .ToAsync()
           .MatchAsync<IActionResult>(
               async confirmation =>
               {
                   var result = confirmation.MarkValidated(request.EmailConfirmationToken, DateTimeOffset.UtcNow, _otpSettings.Disabled);

                   if (result.HasValue)
                   {
                       await _mdpUnitOfWork.Commit();

                       return BadRequest(ApiError.From(result.Value.Message, result.Value.Message));
                   }

                   var businesGroup2faStatus = await _tenantRepository.GetBusinessGroupStatus(HttpContext.User.User().BusinessGroup);

                   var currentMember = (await _memberRepository.FindMember(
                       HttpContext.User.User().ReferenceNumber,
                       HttpContext.User.User().BusinessGroup,
                       businesGroup2faStatus
                       )).Value();

                   var email = Email.Create(confirmation.Contact).Right();
                   if (currentMember.HasEmail(email))
                   {
                       await _mdpUnitOfWork.Commit();
                       return NoContent();
                   }

                   var multiDbTransaction = await new MultiDbTransaction(_memberDbUnitOfWork, _mdpUnitOfWork).Begin();
                   var now = DateTimeOffset.UtcNow;
                   var addressNumber = await _systemRepository.NextAddressNumber();
                   currentMember
                    .SaveEmail(
                       email,
                       await _systemRepository.NextAuthorizationNumber(),
                       addressNumber,
                       await _objectStatusRepository.FindTableShort(HttpContext.User.User().BusinessGroup),
                       now)
                    .UpdateEmailValidationFor2FaBusinessGroup(
                       HttpContext.User.User().UserId,
                       addressNumber,
                       request.EmailConfirmationToken,
                       businesGroup2faStatus,
                       now);

                   try
                   {
                       await _mdpUnitOfWork.Commit();
                       await _memberDbUnitOfWork.Commit();

                       //TODO: update global var in oracle
                       //TODO: revert transaction if unable to update global var

                       await multiDbTransaction.Commit();
                       return NoContent();
                   }
                   catch
                   {
                       await multiDbTransaction.Rollback();
                       throw;
                   }
               },
               () => NotFound(ApiError.NotFound()));
    }

    [HttpPost]
    [Route("api/v2/members/contacts/confirmation/mobile-phone/send")]
    [ProducesResponseType(typeof(ContactConfirmationResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> SendConfirmationSms([FromBody] SendMobilePhoneConfirmationV2Request request)
    {
        var utcNow = DateTimeOffset.UtcNow;

        var member = await _memberRepository.FindMember(HttpContext.User.User().ReferenceNumber, HttpContext.User.User().BusinessGroup);
        if (member.IsNone)
            return BadRequest(ApiError.FromMessage("Member was not found."));

        var mobilePhoneOrError = Phone.Create(request.Code, request.Number);
        if (mobilePhoneOrError.IsLeft)
            return BadRequest(ApiError.FromMessage(mobilePhoneOrError.Left().Message));

        // TODO: check maybe saving smsId would be useful here
        var confirmation = ContactConfirmation.CreateForMobile(
            HttpContext.User.User().BusinessGroup,
            HttpContext.User.User().ReferenceNumber,
            RandomNumber.Get(),
            mobilePhoneOrError.Right(),
            utcNow.AddMinutes(_contactsConfirmationConfiguration.MobilePhoneTokenExpiresInMin),
            utcNow,
            _contactsConfirmationConfiguration.MaxMobileConfirmationAttemptCount);

        var template = await _contentClient.FindTemplate("sms_confirmation_message", request.ContentAccessKey, $"{member.Value().SchemeCode}-{member.Value().Category}");
        var body = new Template(template.HtmlBody).Apply(new Dictionary<string, string>()
        {
            ["security_token"] = confirmation.Token
        });

        await using var transaction = await _memberDbUnitOfWork.BeginTransactionAsync();
        try
        {
            var response = _messageClient.SendMessage(template.EmailFrom, body, new[] { confirmation.Contact });

            if (response.IsLeft)
            {
                await transaction.RollbackAsync();
                return BadRequest(ApiError.FromMessage("SMS message was not sent."));
            }

            await _contactConfirmationRepository.Create(confirmation);
            await _mdpUnitOfWork.Commit();
        }
        catch (OverflowException)
        {
            await transaction.RollbackAsync();
            _logger.LogError("Phone number was either too large or too small");
            return BadRequest(ApiError.FromMessage("Phone number was either too large or too small"));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return Ok(ContactConfirmationResponse.From(confirmation.ExpiresAt));
    }

    [HttpPost]
    [Route("api/members/contacts/confirmation/mobile-phone/confirm")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [TypeFilter(typeof(ResponseHashFilterAttribute))]
    public async Task<IActionResult> ConfirmMobilePhone(MobilePhoneConfirmationRequest request)
    {
        return await _contactConfirmationRepository.FindLastMobilePhoneConfirmation(
               HttpContext.User.User().BusinessGroup,
               HttpContext.User.User().ReferenceNumber)
           .ToAsync()
           .MatchAsync<IActionResult>(
               async confirmation =>
               {
                   var result = confirmation.MarkValidated(request.MobilePhoneConfirmationToken, DateTimeOffset.UtcNow, _otpSettings.Disabled);

                   if (result.HasValue)
                   {
                       await _mdpUnitOfWork.Commit();

                       return BadRequest(ApiError.From(result.Value.Message, result.Value.Message));
                   }

                   var businesGroup2faStatus = await _tenantRepository.GetBusinessGroupStatus(HttpContext.User.User().BusinessGroup);

                   var currentMember = (await _memberRepository.FindMember(
                       HttpContext.User.User().ReferenceNumber,
                       HttpContext.User.User().BusinessGroup,
                       businesGroup2faStatus)).Value();

                   var mobilePhoneOrError = Phone.Create(confirmation.Contact);

                   if (mobilePhoneOrError.IsLeft)
                       return BadRequest(ApiError.FromMessage(mobilePhoneOrError.Left().Message));
                   var mobilePhone = mobilePhoneOrError.Right();

                   if (currentMember.HasMobilePhone(mobilePhone))
                   {
                       await _mdpUnitOfWork.Commit();
                       return NoContent();
                   }

                   await using var transaction = await _memberDbUnitOfWork.BeginTransactionAsync();
                   var now = DateTimeOffset.UtcNow;
                   //await _memberServiceClient.UpdateMobilePhone(HttpContext.User.User().BusinessGroup,
                   //                                       HttpContext.User.User().ReferenceNumber,
                   //                                       mobilePhone);
                   var addressNumber = await _systemRepository.NextAddressNumber();


                   var savedOrError = currentMember
                    .SaveMobilePhone(
                       mobilePhone,
                       await _systemRepository.NextAuthorizationNumber(),
                       addressNumber,
                       await _objectStatusRepository.FindTableShort(HttpContext.User.User().BusinessGroup),
                       now)
                    .UpdateMobilePhoneValidationFor2FaBusinessGroup(
                       HttpContext.User.User().UserId,
                       addressNumber,
                       request.MobilePhoneConfirmationToken,
                       businesGroup2faStatus,
                       now, request.MobilePhoneCountry);

                   if (savedOrError.IsLeft)
                   {
                       return BadRequest(ApiError.FromMessage(savedOrError.Left().Message));
                   }

                   try
                   {
                       await _mdpUnitOfWork.Commit();
                       await _memberDbUnitOfWork.Commit();

                       //TODO: update global var in oracle
                       //TODO: revert transaction if unable to update global var

                       await transaction.CommitAsync();
                       return NoContent();
                   }
                   catch (Exception)
                   {
                       await transaction.RollbackAsync();
                       throw;
                   }
               },
               () => NotFound(ApiError.NotFound()));
    }
}