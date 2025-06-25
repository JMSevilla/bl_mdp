using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WTW.MdpService.BereavementJourneys;
using WTW.MdpService.ContactsConfirmation;
using WTW.MdpService.Domain;
using WTW.MdpService.Domain.Bereavement;
using WTW.MdpService.Infrastructure.BereavementDb;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.EmailConfirmation;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;
using WTW.Web.Templates;

namespace WTW.MdpService.BereavementContactsConfirmation;

[ApiController]
[Authorize(Policy = "BereavementInitialUserOrMember")]
public class BereavementContactsConfirmationController : ControllerBase
{
    private readonly BereavementJourneyConfiguration _bereavementJourneyConfiguration;
    private readonly IContentClient _contentClient;
    private readonly IEmailConfirmationSmtpClient _emailConfirmationSmtpClient;
    private readonly IBereavementUnitOfWork _bereavementUnitOfWork;
    private readonly OtpSettings _otpSettings;
    private readonly IBereavementContactConfirmationRepository _repository;

    public BereavementContactsConfirmationController(
        BereavementJourneyConfiguration bereavementJourneyConfiguration,
        IContentClient contentClient,
        IEmailConfirmationSmtpClient emailConfirmationSmtpClient,
        IBereavementUnitOfWork bereavementUnitOfWork,
        OtpSettings otpSettings,
        IBereavementContactConfirmationRepository repository)
    {
        _bereavementJourneyConfiguration = bereavementJourneyConfiguration;
        _contentClient = contentClient;
        _emailConfirmationSmtpClient = emailConfirmationSmtpClient;
        _bereavementUnitOfWork = bereavementUnitOfWork;
        _otpSettings = otpSettings;
        _repository = repository;
    }

    [HttpPost("api/bereavement/contacts/confirmation/email/send")]
    [ProducesResponseType(typeof(BereavementContactsConfirmationResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> SendConfirmationEmail([FromBody] BereavementContactsConfirmationRequest request)
    {
        var (bereavementReferenceNumber, businessGroup) = HttpContext.User.BereavementUser();
        var utcNow = DateTimeOffset.UtcNow;

        var emailOrError = Email.Create(request.EmailAddress);
        if (emailOrError.IsLeft)
            return BadRequest(ApiError.FromMessage(emailOrError.Left().Message));

        var existingConfirmation = await _repository.FindLocked(emailOrError.Right());

        if (existingConfirmation.IsSome && !existingConfirmation.Value().IsLockExpired(utcNow, _bereavementJourneyConfiguration.EmailLockPeriodInMin))
        {
            return BadRequest(ApiError.From(
                $"Due to max failure attempts this email is locked until: " +
                    $"{existingConfirmation.Value().LockReleaseDate(_bereavementJourneyConfiguration.EmailLockPeriodInMin)}",
                "EMAIL_IS_LOCKED"));
        }

        var confirmation = BereavementContactConfirmation.CreateForEmail(
                       businessGroup,
                       bereavementReferenceNumber,
                       RandomNumber.Get(),
                       emailOrError.Right(),
                       utcNow.AddMinutes(_bereavementJourneyConfiguration.EmailTokenExpiresInMin),
                       utcNow,
                       _bereavementJourneyConfiguration.MaxEmailConfirmationAttemptCount);

        var template = await _contentClient.FindTemplate("email_validation_message", request.ContentAccessKey);
        var htmlBody = new Template(template.HtmlBody).Apply(new Dictionary<string, string>()
        {
            ["security_token"] = confirmation.Token
        });

        await _repository.Create(confirmation);
        await _bereavementUnitOfWork.Commit();
        await _emailConfirmationSmtpClient.Send(emailOrError.Right(), template.EmailFrom, htmlBody, template.EmailSubject);
        return Ok(BereavementContactsConfirmationResponse.From(confirmation.ExpiresAt));
    }

    [HttpPost("api/bereavement/contacts/confirmation/email/confirm")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> ConfirmEmail(EmailConfirmationRequest request)
    {
        return await _repository.FindLastEmailConfirmation(
              HttpContext.User.BereavementUser().BusinessGroup,
              HttpContext.User.BereavementUser().BereavementReferenceNumber)
          .ToAsync()
          .MatchAsync<IActionResult>(
              async confirmation =>
              {
                  var result = confirmation.MarkValidated(request.EmailConfirmationToken, DateTimeOffset.UtcNow, _otpSettings.Disabled);

                  if (result.HasValue)
                  {
                      await _bereavementUnitOfWork.Commit();

                      return BadRequest(ApiError.From(result.Value.Message, result.Value.Message));
                  }

                  _repository.Remove(confirmation);
                  await _bereavementUnitOfWork.Commit();
                  return NoContent();

              },
              () => NotFound(ApiError.NotFound()));
    }
}