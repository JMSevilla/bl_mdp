using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.Web.Authorization;
using WTW.Web.Caching;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Contacts;

[ApiController]
[Route("api/members/current/contacts")]
public class ContactsController : ControllerBase
{
    private readonly IMemberRepository _memberRepository;
    private readonly ISystemRepository _systemRepository;
    private readonly IObjectStatusRepository _objectStatusRepository;
    private readonly IMemberDbUnitOfWork _uow;
    private readonly ILogger<ContactsController> _logger;
    private readonly ITenantRepository _tenantRepository;

    public ContactsController(
        IMemberRepository memberRepository,
        ISystemRepository systemRepository,
        IObjectStatusRepository objectStatusRepository,
        IMemberDbUnitOfWork uow,
        ILogger<ContactsController> logger,
        ITenantRepository tenantRepository)
    {
        _memberRepository = memberRepository;
        _systemRepository = systemRepository;
        _objectStatusRepository = objectStatusRepository;
        _uow = uow;
        _logger = logger;
        _tenantRepository = tenantRepository;
    }

    [HttpGet("email")]
    [ProducesResponseType(typeof(EmailResponse), 200)]
    [SwaggerResponse(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> GetCurrentEmail()
    {
        (string _, string referenceNumber, string businessGroup) = HttpContext.User.User();

        return await _memberRepository.FindMember(referenceNumber, businessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async member =>
                {
                    var email = member.Email();
                    if (email.IsNone)
                        return NoContent();

                    return Ok(EmailResponse.From(email.Single()));
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("mobile-phone")]
    [ProducesResponseType(typeof(MobilePhoneResponse), 200)]
    [SwaggerResponse(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> CurrentMobilePhone()
    {
        return await _memberRepository.FindMember(HttpContext.User.User().ReferenceNumber, HttpContext.User.User().BusinessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async member =>
                {
                    var code = member.MobilePhoneCode();
                    var number = member.MobilePhoneNumber();
                    if (code.IsNone || number.IsNone)
                        return NoContent();

                    return Ok(MobilePhoneResponse.From(code.Single(), number.Single()));
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("notifications-settings")]
    [ProducesResponseType(typeof(NotificationSettingsResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> RetrieveNotificationsSettings()
    {
        return await _memberRepository.FindMember(HttpContext.User.User().ReferenceNumber, HttpContext.User.User().BusinessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async member =>
                {
                    var preferences = member.NotificationsSettings();
                    return Ok(new NotificationSettingsResponse(preferences.Email, preferences.Sms, preferences.Post));
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpPut("notifications-settings")]
    [ProducesResponseType(typeof(NotificationSettingsResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> UpdateNotificationsSettings([FromBody] NotificationSettingsRequest request)
    {
        return await _memberRepository.FindMember(HttpContext.User.User().ReferenceNumber, HttpContext.User.User().BusinessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async member =>
                {
                    var scheme = member.NotificationSettings.SingleOrDefault(x => x.EndDate == null && x.Scheme == "M")?.Scheme;
                    _logger.LogInformation("Preparing notification settings update for {MemberReferenceNumber}, {MemberBusinessGroup}, {MemberScheme}", member.ReferenceNumber, member.BusinessGroup, scheme);
                    var result = member.UpdateNotificationsSettings(request.TypeToUpdate.ToString(), request.Email, request.Sms, request.Post, DateTimeOffset.UtcNow);
                    if (result.IsLeft)
                        return BadRequest(ApiError.FromMessage(result.Left().Message));

                    _logger.LogInformation("Committing notification settings update for {MemberReferenceNumber}, {MemberBusinessGroup}, {MemberScheme} with sequence nr: {SequenceNumber}", member.ReferenceNumber, member.BusinessGroup, scheme, result.Right());
                    await _uow.Commit();
                    var preferences = member.NotificationsSettings();
                    return Ok(new NotificationSettingsResponse(preferences.Email, preferences.Sms, preferences.Post));
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpPut("address")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> SaveAddress(AddressRequest request)
    {
        return await _memberRepository.FindMember(HttpContext.User.User().ReferenceNumber, HttpContext.User.User().BusinessGroup)
           .ToAsync()
           .MatchAsync<IActionResult>(
               async member =>
               {
                   var addressOrError = Address.Create(request.StreetAddress1,
                       request.StreetAddress2,
                       request.StreetAddress3,
                       request.StreetAddress4,
                       request.StreetAddress5,
                       request.Country,
                       request.CountryCode,
                       request.PostCode);

                   if (addressOrError.IsLeft)
                       return BadRequest(ApiError.FromMessage(addressOrError.Left().Message));

                   if (member.HasAddress(addressOrError.Right()))
                       return NoContent();

                   await using var transaction = await _uow.BeginTransactionAsync();
                   await _memberRepository.PopulateSessionDetails(member.BusinessGroup);
                   member.SaveAddress(addressOrError.Right(),
                        await _systemRepository.NextAuthorizationNumber(),
                        await _systemRepository.NextAddressNumber(),
                        await _objectStatusRepository.FindTableShort(HttpContext.User.User().BusinessGroup),
                        DateTimeOffset.UtcNow);

                   try
                   {
                       await _uow.Commit();

                       //TODO: update global var in oracle
                       //TODO: revert transaction if unable to update global var

                       await transaction.CommitAsync();
                   }
                   catch (Exception)
                   {
                       await transaction.RollbackAsync();
                       throw;
                   }

                   await _memberRepository.DisableSysAudit(member.BusinessGroup);
                   return NoContent();
               },
               () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("address")]
    [ProducesResponseType(typeof(MemberAddressResponse), 200)]
    [SwaggerResponse(404, "Not Found. Available error codes: member_address_not_found", typeof(ApiError))]
    public async Task<IActionResult> RetrieveAddress()
    {
        return (await _memberRepository.FindMember(HttpContext.User.User().ReferenceNumber, HttpContext.User.User().BusinessGroup))
            .Match<IActionResult>(
                member =>
                {
                    var result = member.Address();
                    if (result.IsNone)
                        return NotFound(ApiError.From("Member does not have any address.", "member_address_not_found"));

                    return Ok(MemberAddressResponse.From(result.Value()));
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("/api/contact/countries")]
    [ProducesResponseType(typeof(IEnumerable<GetContactCountriesResponse>), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [CacheResponseAttribute("contactcountries", 864000)] // 10 days
    [Authorize(Policy = "BereavementInitialUserOrMember")]
    public async Task<IActionResult> GetContactCountries()
    {
        var response = await _tenantRepository.GetAddressCountries("IDDCD", "ZZY");
        return Ok(GetContactCountriesResponse.From(response));
    }
}