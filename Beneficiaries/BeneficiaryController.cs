using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Members.Beneficiaries;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.EmailConfirmation;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.MdpService.Infrastructure.PdfGenerator;
using WTW.MdpService.Infrastructure.Templates.Beneficiaries;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.Extensions;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Beneficiaries;

[ApiController]
[Route("api/members/current/beneficiaries")]
public class BeneficiaryController : ControllerBase
{
    private readonly IMemberRepository _memberRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IEmailConfirmationSmtpClient _smtpClient;
    private readonly IMemberDbUnitOfWork _memberDbUnitOfWork;
    private readonly IContentClient _contentClient;
    private readonly ILogger<BeneficiaryController> _logger;
    private readonly IPdfGenerator _pdfGenerator;

    public BeneficiaryController(IMemberRepository memberRepository,
                                ITenantRepository tenantRepository,
                                IEmailConfirmationSmtpClient smtpClient,
                                IMemberDbUnitOfWork memberDbUnitOfWork,
                                IContentClient contentClient,
                                ILogger<BeneficiaryController> logger,
                                IPdfGenerator pdfGenerator)
    {
        _memberRepository = memberRepository;
        _tenantRepository = tenantRepository;
        _smtpClient = smtpClient;
        _memberDbUnitOfWork = memberDbUnitOfWork;
        _contentClient = contentClient;
        _logger = logger;
        _pdfGenerator = pdfGenerator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(BeneficiariesResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> Beneficiaries()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        return await _memberRepository.FindMemberWithBeneficiaries(referenceNumber, businessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async member => Ok(BeneficiariesResponse.From(member.ActiveBeneficiaries())),
                () => NotFound(ApiError.NotFound()));
    }

    [HttpPut]
    [ProducesResponseType(typeof(BeneficiariesResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> UpdateBeneficiaries([FromBody] UpdateBeneficiariesRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        return await _memberRepository.FindMemberWithBeneficiaries(referenceNumber, businessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async member =>
                {
                    var statuses = await SupportedRelationships(businessGroup);
                    if (!statuses.ContainsAllItems(request.Beneficiaries.Select(x => x.Relationship)))
                        return BadRequest(ApiError.FromMessage("Unsupported relationship."));

                    var beneficiaries = DomainBeneficiaries(request);

                    var errors = AggregateErrors(beneficiaries);
                    if (errors.Any())
                        return BadRequest(ApiError.FromMessage(errors.First().Message));

                    using var transaction = await _memberDbUnitOfWork.BeginTransactionAsync();
                    try
                    {
                        await _memberRepository.PopulateSessionDetails(businessGroup);

                        var result = member.UpdateBeneficiaries(beneficiaries.Select(x => (x.Id, x.Details.Right(), x.Addresses.Right())).ToList(), DateTimeOffset.UtcNow);
                        if (result.HasValue)
                            return BadRequest(ApiError.FromMessage(result.Value.Message));

                        await _memberDbUnitOfWork.Commit();

                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to update beneficiaries.");
                        await transaction.RollbackAsync();
                        throw;
                    }

                    await _memberRepository.DisableSysAudit(businessGroup);

                    await member.Email().IfSomeAsync(async email =>
                    {
                        try
                        {
                            var template = await _contentClient.FindTemplate("beneficiaries_save_email", request.ContentAccessKey, $"{member.SchemeCode}-{member.Category}");
                            var html = await BeneficiariesUpdateEmailTemplate.RenderHtml(template.HtmlBody, member.PersonalDetails.Forenames);
                            var emailSubject = await BeneficiariesUpdateEmailTemplate.RenderHtml(template.EmailSubject, member.PersonalDetails.Forenames);
                            await _smtpClient.Send(email, template.EmailFrom, html, emailSubject);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send beneficiaries updates email.");
                        }
                    });

                    return Ok(BeneficiariesResponse.From(member.ActiveBeneficiaries()));
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("download")]
    [ProducesResponseType(typeof(FileResult), 200, "application/octet-stream")]
    [ProducesResponseType(typeof(ApiError), 404, "application/json")]
    public async Task<IActionResult> DownloadBeneficiariesPdf([FromQuery][Required] string contentAccessKey)
    {
        return await _memberRepository.FindMemberWithBeneficiaries(HttpContext.User.User().ReferenceNumber, HttpContext.User.User().BusinessGroup)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async member =>
                {
                    if (!member.ActiveBeneficiaries().Any())
                        return NotFound(ApiError.NotFound());

                    var template = await _contentClient.FindTemplate("beneficiaries_pdf", contentAccessKey, $"{member.SchemeCode}-{member.Category}");
                    var html = await BeneficiariesTemplate.RenderHtml(template.HtmlBody, member.ActiveBeneficiaries());

                    return File(
                        await _pdfGenerator.Generate(html, template.HtmlHeader, template.HtmlFooter),
                        "application/pdf",
                        $"{template.TemplateName.Replace(' ', '_').ToLower()}.pdf");
                },
                () => NotFound(ApiError.NotFound()));
    }

    private static List<Error> AggregateErrors(List<(int? SequenceNumber, Either<Error, BeneficiaryDetails> Details, Either<Error, BeneficiaryAddress> Addresses)> beneficiaries)
    {
        return beneficiaries.Where(x => x.Details.IsLeft || x.Addresses.IsLeft).Select(x =>
        {
            if (x.Addresses.IsLeft)
                return x.Addresses.Left();

            return x.Details.Left();
        }).ToList();
    }

    private static List<(int? Id, Either<Error, BeneficiaryDetails> Details, Either<Error, BeneficiaryAddress> Addresses)> DomainBeneficiaries(UpdateBeneficiariesRequest request)
    {
        return request.Beneficiaries
            .Select(b =>
                b.Relationship == BeneficiaryDetails.CharityStatus
                    ? (b.Id, BeneficiaryDetails.CreateCharity(b.CharityName, b.CharityNumber, b.LumpSumPercentage.Value, b.Notes), BeneficiaryAddress.Empty())
                    : (
                        b.Id,
                        BeneficiaryDetails.CreateNonCharity(b.Relationship, b.Forenames, b.Surname, b.DateOfBirth, b.LumpSumPercentage.Value, b.IsPensionBeneficiary, b.Notes),
                        BeneficiaryAddress.Create(b.Address.Line1, b.Address.Line2, b.Address.Line3, b.Address.Line4, b.Address.Line5, b.Address.Country, b.Address.CountryCode, b.Address.PostCode)
                        )
            ).ToList();
    }

    private async Task<List<string>> SupportedRelationships(string businessGroup)
    {
        var statuses = (await _tenantRepository.ListRelationships(businessGroup)).Select(x => x.ListValue).ToList();
        statuses.Add(BeneficiaryDetails.CharityStatus);
        return statuses;
    }
}