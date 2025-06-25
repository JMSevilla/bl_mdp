using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt.UnsafeValueAccess;
using MessageBird.Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Infrastructure.Aws;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.Edms;
using WTW.MdpService.Infrastructure.EmailConfirmation;
using WTW.MdpService.Infrastructure.JobScheduler;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.MdpService.Infrastructure.MemberDb.Documents;
using WTW.MdpService.Infrastructure.MemberDb.IfaReferrals;
using WTW.MdpService.Infrastructure.PdfGenerator;
using WTW.MdpService.Infrastructure.Redis;
using WTW.MdpService.Infrastructure.Templates.Ifa;
using WTW.MdpService.Infrastructure.Templates.TransferApplication;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.TransferJourneys;

[ApiController]
[Route("api/transfer-journeys")]
public class TransferJourneyController : ControllerBase
{
    private readonly ICalculationsClient _calculationsClient;
    private readonly ITransferJourneyRepository _transferJourneyRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IMdpUnitOfWork _mdpDbUnitOfWork;
    private readonly IMemberDbUnitOfWork _uow;
    private readonly IContentClient _contentClient;
    private readonly IEdmsClient _edmsClient;
    private readonly IEmailConfirmationSmtpClient _emailConfirmationSmtpClient;
    private readonly ILogger<TransferJourneyController> _logger;
    private readonly IRetirementPostIndexEventRepository _postIndexEventsRepository;
    private readonly IJobSchedulerClient _jobSchedulerClient;
    private readonly ICalculationsRedisCache _calculationsRedisCache;
    private readonly IAwsClient _awsClient;
    private readonly ITransferCalculationRepository _transferCalculationRepository;
    private readonly JobSchedulerConfiguration _jobSchedulerConfiguration;
    private readonly ICalculationHistoryRepository _calculationHistoryRepository;
    private readonly IIfaConfigurationRepository _ifaConfigurationRepository;
    private readonly MemberIfaReferral _memberIfaReferral;
    private readonly IPdfGenerator _pdfGenerator;
    private readonly ICalculationsParser _calculationsParser;

    public TransferJourneyController(
        ICalculationsClient calculationsClient,
        ITransferJourneyRepository transferJourneyRepository,
        IMemberRepository memberRepository,
        IMdpUnitOfWork mdpDbUnitOfWork,
        IMemberDbUnitOfWork uow,
        IContentClient contentClient,
        IEdmsClient edmsClient,
        IEmailConfirmationSmtpClient emailConfirmationSmtpClient,
        ILogger<TransferJourneyController> logger,
        IRetirementPostIndexEventRepository postIndexEventsRepository,
        IJobSchedulerClient jobSchedulerClient,
        ICalculationsRedisCache calculationsRedisCache,
        IAwsClient awsClient,
        ITransferCalculationRepository transferCalculationRepository,
        JobSchedulerConfiguration jobSchedulerConfiguration,
        ICalculationHistoryRepository calculationHistoryRepository,
        IIfaConfigurationRepository ifaConfigurationRepository,
        MemberIfaReferral memberIfaReferral,
        IPdfGenerator pdfGenerator,
        ICalculationsParser calculationsParser)
    {
        _calculationsClient = calculationsClient;
        _transferJourneyRepository = transferJourneyRepository;
        _memberRepository = memberRepository;
        _mdpDbUnitOfWork = mdpDbUnitOfWork;
        _uow = uow;
        _contentClient = contentClient;
        _edmsClient = edmsClient;
        _emailConfirmationSmtpClient = emailConfirmationSmtpClient;
        _logger = logger;
        _postIndexEventsRepository = postIndexEventsRepository;
        _jobSchedulerClient = jobSchedulerClient;
        _calculationsRedisCache = calculationsRedisCache;
        _awsClient = awsClient;
        _transferCalculationRepository = transferCalculationRepository;
        _jobSchedulerConfiguration = jobSchedulerConfiguration;
        _calculationHistoryRepository = calculationHistoryRepository;
        _ifaConfigurationRepository = ifaConfigurationRepository;
        _memberIfaReferral = memberIfaReferral;
        _pdfGenerator = pdfGenerator;
        _calculationsParser = calculationsParser;
    }

    [HttpPost("start")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> StartTransferJourney([FromBody] StartTransferJourneyRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        var result = await _transferJourneyRepository.Find(businessGroup, referenceNumber);
        if (result.IsSome)
            _transferJourneyRepository.Remove(result.Value());

        var hardQuoteResult = await _calculationsClient.HardQuote(businessGroup, referenceNumber);
        if (hardQuoteResult.IsLeft)
            return BadRequest(ApiError.FromMessage(hardQuoteResult.Left().Message));

        var transferCalculation = await _transferCalculationRepository.Find(businessGroup, referenceNumber);
        transferCalculation.Value().LockTransferQoute();
        await _calculationsRedisCache.ClearRetirementDateAges(referenceNumber, businessGroup);

        var awsResult = await _awsClient.File(hardQuoteResult.Right());
        if (awsResult.IsLeft)
            return BadRequest(ApiError.FromMessage(awsResult.Left().Message));

        var edmsResult = await _edmsClient.PreindexDocument(
            businessGroup,
            referenceNumber,
            $"{businessGroup}1",
            awsResult.Right());
        if (edmsResult.IsLeft)
        {
            transferCalculation.Value().UnlockTransferQoute();
            await _mdpDbUnitOfWork.Commit();
            return BadRequest(ApiError.FromMessage($"EDMS preIndex failed: {edmsResult.Left().Message}"));
        }

        var typeResponse = (await _calculationsClient.TransferEventType(businessGroup, referenceNumber).Try()).Value();

        _logger.LogInformation("Calling CalcApi to get LockedInTransferQuoteSeqno with business group:{businessGroup} and refno:{referenceNumber}", businessGroup, referenceNumber);
        var datesAgesResponse = await _calculationsClient.RetirementDatesAges(referenceNumber, businessGroup).Try();
        if (!datesAgesResponse.IsSuccess)
        {
            _logger.LogError("Failed to retrieve LockedInTransferQuoteSeqno value from calc api. Business group:{businessGroup}, Refno:{referenceNumber}.", businessGroup, referenceNumber);
            return BadRequest(ApiError.FromMessage("Failed to receive transfer quote information"));
        }

        if (datesAgesResponse.Value().LockedInTransferQuoteSeqno == null)
        {
            _logger.LogError("LockedInTransferQuoteSeqno value is null. Business group:{businessGroup}, Refno:{referenceNumber}", businessGroup, referenceNumber);
            return BadRequest(ApiError.FromMessage("Seqno does not have value"));
        }

        var calculationHistory = await _calculationHistoryRepository.FindByEventTypeAndSeqNumber(businessGroup, referenceNumber, typeResponse.Type, datesAgesResponse.Value().LockedInTransferQuoteSeqno.Value);

        if (calculationHistory.IsNone)
        {
            transferCalculation.Value().UnlockTransferQoute();
            await _mdpDbUnitOfWork.Commit();
            return BadRequest(ApiError.FromMessage("Calc history record cannnot be found"));
        }

        calculationHistory.Value().UpdateIds(edmsResult.Right().ImageId, edmsResult.Right().BatchNumber);

        _postIndexEventsRepository.Add(
                new RetirementPostIndexEvent(
                    businessGroup,
                    referenceNumber,
                    "TRANSFER",
                    edmsResult.Right().BatchNumber,
                    edmsResult.Right().ImageId));

        var journey = TransferJourney.Create(
            businessGroup,
            referenceNumber,
            DateTimeOffset.UtcNow,
            request.CurrentPageKey,
            request.NextPageKey,
            edmsResult.Right().ImageId);
        await _transferJourneyRepository.Create(journey);
        await _mdpDbUnitOfWork.Commit();
        await _uow.Commit();
        return NoContent();
    }

    [HttpGet("transfer-values")]
    [ProducesResponseType(typeof(PensionIncomeResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> TransferValues([FromQuery][Required] decimal requestedResidualPension)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        var result = await _transferJourneyRepository.Find(businessGroup, referenceNumber);
        if (!result.IsSome)
            return BadRequest(ApiError.FromMessage("Transfer journey not started."));

        return (await _calculationsClient.TransferValues(businessGroup, referenceNumber, requestedResidualPension))
            .Match<IActionResult>(
                response => Ok(PensionIncomeResponse.From(response)),
                error => BadRequest(ApiError.FromMessage(error.Message)));
    }

    [HttpGet("download/partial-transfer-pdf")]
    [ProducesResponseType(typeof(FileResult), 200, "application/octet-stream")]
    public async Task<IActionResult> PartialTransferPdf([FromQuery] PartialTrasferPdfRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        
        var member = await _memberRepository.FindMember(referenceNumber, businessGroup);
        if (member.IsNone)
            return BadRequest(ApiError.FromMessage("Member was not found."));

        var ptTransferValues = await _calculationsClient
                .PartialTransferValues(businessGroup, referenceNumber, request.RequestedTransferValue, request.RequestedResidualPension);

        if (ptTransferValues.IsLeft)
            _logger.LogWarning(ptTransferValues.Left().Message);

        var template = await _contentClient.FindTemplate("partial_transfer_pdf", request.ContentAccessKey, $"{member.Value().SchemeCode}-{member.Value().Category}");
        var html = await TransferTemplate.RenderHtml(template.HtmlBody, ptTransferValues.Right(), referenceNumber, DateTimeOffset.UtcNow);
        var pdf = await _pdfGenerator.Generate(html, template.HtmlHeader, template.HtmlFooter);

        var uploadDocument = await _edmsClient.PreindexDocument(
                        businessGroup,
                        referenceNumber,
                        $"{businessGroup}1",
                        pdf);
        if (!uploadDocument.IsRight)
            _logger.LogError($"EDMS preIndex failed: {uploadDocument.Left()}");

        return File(pdf.ToArray(), "application/pdf", "partial-transfer.pdf");
    }

    [HttpGet("download/transfer-pdf")]
    [ProducesResponseType(typeof(FileResult), 200, "application/octet-stream")]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> TransferPdf()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        var transfer = await _transferJourneyRepository.Find(businessGroup, referenceNumber);
        if (!transfer.IsSome)
            return BadRequest(ApiError.FromMessage("Transfer journey not started."));

        var fileOrError = await _edmsClient.GetDocumentOrError(transfer.Value().TransferImageId);
        if (fileOrError.IsLeft)
            return BadRequest(ApiError.FromMessage(fileOrError.Left().Message));

        return File(fileOrError.Right(), "application/octet-stream", "transfer-journey.pdf");
    }

    [HttpPost("emails/ifa/send")]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> SendIfaEmails([FromBody][Required] SendIfaEmailsRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        var member = (await _memberRepository.FindMember(referenceNumber, businessGroup)).SingleOrDefault();
        if (member == null)
            return BadRequest(ApiError.FromMessage("Member was not found."));

        var transfer = await _transferJourneyRepository.Find(businessGroup, referenceNumber);
        if (!transfer.IsSome)
            return BadRequest(ApiError.FromMessage("Transfer journey not started."));

        var transferCalculation = await _transferCalculationRepository.Find(businessGroup, referenceNumber);

        var loginResult = await _jobSchedulerClient.Login();
        if (loginResult.IsLeft)
            return BadRequest(ApiError.FromMessage(loginResult.Left().Message));

        var typeResponse = (await _calculationsClient.TransferEventType(businessGroup, referenceNumber).Try()).Value();
        await _calculationsRedisCache.ClearRetirementDateAges(referenceNumber, businessGroup);

        _logger.LogInformation("Calling CalcApi to get LockedInTransferQuoteSeqno with business group:{businessGroup} and refno:{referenceNumber}", businessGroup, referenceNumber);
        var datesAgesResponse = await _calculationsClient.RetirementDatesAges(referenceNumber, businessGroup).Try();
        if (!datesAgesResponse.IsSuccess)
        {
            _logger.LogError("Failed to retrieve LockedInTransferQuoteSeqno value from calc api. Business group:{businessGroup}, Refno:{referenceNumber}.", businessGroup, referenceNumber);
            return BadRequest(ApiError.FromMessage("Failed to receive transfer quote information"));
        }

        if (datesAgesResponse.Value().LockedInTransferQuoteSeqno == null)
        {
            _logger.LogError("LockedInTransferQuoteSeqno value is null. Business group:{businessGroup}, Refno:{referenceNumber}", businessGroup, referenceNumber);
            return BadRequest(ApiError.FromMessage("Seqno does not have value"));
        }

        var orderRequest = OrderRequest.CreateOrderRequest(referenceNumber, businessGroup, typeResponse.Type, _jobSchedulerConfiguration.JobChainEnv, datesAgesResponse.Value().LockedInTransferQuoteSeqno.Value);
        var orderRequestResult = await _jobSchedulerClient.CreateOrder(orderRequest, loginResult.Right().AccessToken);
        if (orderRequestResult.IsLeft)
        {
            await _jobSchedulerClient.Logout(loginResult.Right().AccessToken);
            return BadRequest(ApiError.FromMessage(orderRequestResult.Left().Message));
        }

        var orderStatusResult = await _jobSchedulerClient.CheckOrderStatus(OrderRequest.OrderStatusRequest(orderRequest.Orders.FirstOrDefault()?.OrderId, _jobSchedulerConfiguration.JobChainEnv), loginResult.Right().AccessToken);
        if (orderStatusResult.IsLeft)
        {
            await _jobSchedulerClient.Logout(loginResult.Right().AccessToken);
            return BadRequest(ApiError.FromMessage(orderRequestResult.Left().Message));
        }

        await _jobSchedulerClient.Logout(loginResult.Right().AccessToken);
        transfer.Value().SetCalculationType(typeResponse.Type);

        var email = member.Email().SingleOrDefault();
        var number = member.FullMobilePhoneNumber();

        var ifaMemberTemplate = await _contentClient.FindTemplate("transfer_completion_email", request.ContentAccessKey, $"{member.SchemeCode}-{member.Category}");
        var ifaCompanyTemplate = await _contentClient.FindTemplate("transfer_lv_email", request.ContentAccessKey, $"{member.SchemeCode}-{member.Category}");

        var ifaMemberEmailBody = await IfaMemberTemplate.Render(ifaMemberTemplate.HtmlBody, member.PersonalDetails);
        var ifaMemberEmailSubject = await IfaMemberTemplate.Render(ifaMemberTemplate.EmailSubject, member.PersonalDetails);

        var transferQuote = _calculationsParser.GetTransferQuote(transferCalculation.Value().TransferQuoteJson);

        var ifaCompanyEmailBody = await IfaCompanyTemplate.Render(
                ifaCompanyTemplate.HtmlBody,
                referenceNumber,
                email,
                member.PersonalDetails,
                number.IsSome ? number.Value() : null,
                transferQuote.OriginalEffectiveDate);

        var ifaCompanyEmailSubject = await IfaCompanyTemplate.Render(
                ifaCompanyTemplate.EmailSubject,
                referenceNumber,
                email,
                member.PersonalDetails,
                number.IsSome ? number.Value() : null,
                transferQuote.OriginalEffectiveDate);

        var ifaCompanyEmail = await _ifaConfigurationRepository.FindEmail(businessGroup, typeResponse.Type, "LV");

        if (!string.IsNullOrEmpty(email) && ifaCompanyEmail.IsSome)
        {
            try
            {
                await _emailConfirmationSmtpClient.Send(
                    email,
                    ifaMemberTemplate.EmailFrom,
                    ifaMemberEmailBody,
                    ifaMemberEmailSubject);

                await _emailConfirmationSmtpClient.Send(
                    ifaCompanyEmail.Value(),
                    ifaCompanyTemplate.EmailFrom,
                    ifaCompanyEmailBody,
                    ifaCompanyEmailSubject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email");
                return BadRequest($"Failed to send email. Error: {ex.Message}");
            }
        }

        await _mdpDbUnitOfWork.Commit();
        await _memberIfaReferral.WaitForIfaReferral(referenceNumber, businessGroup, typeResponse.Type, DateTimeOffset.Now);
        return NoContent();
    }
}