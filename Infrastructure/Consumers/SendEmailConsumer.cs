using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using MassTransit;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.EmailConfirmation;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.MdpService.Infrastructure.PdfGenerator;
using WTW.MdpService.Infrastructure.Templates.SingleAuth;
using WTW.MdpService.SingleAuth.Services;
using WTW.MessageBroker.Common;
using WTW.MessageBroker.Contracts.Mdp;
using WTW.Web;
using WTW.Web.LanguageExt;
using WTW.Web.Logging;

namespace WTW.MdpService.Infrastructure.Consumers;

public class SendEmailConsumer : IConsumer<EmailNotification>
{
    private readonly ILogger<SendEmailConsumer> _logger;
    private readonly IContentClient _contentClient;
    private readonly IEmailConfirmationSmtpClient _smtpClient;
    private readonly IRegistrationEmailTemplate _registrationEmailTemplate;
    private readonly IMemberRepository _memberRepository;
    private readonly ISingleAuthService _singleAuthService;
    private readonly IPdfGenerator _pdfGenerator;
    private readonly IWtwPublisher _publisher;

    public SendEmailConsumer(ILogger<SendEmailConsumer> logger, IContentClient contentClient, IEmailConfirmationSmtpClient emailClient,
                             IRegistrationEmailTemplate registrationEmailTemplate, IMemberRepository memberRepository,
                             ISingleAuthService singleAuthService, IPdfGenerator pdfGenerator, IWtwPublisher publisher)
    {
        _logger = logger;
        _contentClient = contentClient;
        _smtpClient = emailClient;
        _registrationEmailTemplate = registrationEmailTemplate;
        _memberRepository = memberRepository;
        _singleAuthService = singleAuthService;
        _pdfGenerator = pdfGenerator;
        _publisher = publisher;
    }

    public async Task Consume(ConsumeContext<EmailNotification> context)
    {
        using (LogsConfiguration.PushProperty(MdpConstants.Bgroup, context.Message.Bgroup))
        using (LogsConfiguration.PushProperty(MdpConstants.Refno, context.Message.Refno))
        using (LogsConfiguration.PushProperty(MdpConstants.CorrelationId, context.CorrelationId.ToString()))
        {
            try
            {

                _logger.LogInformation($"{nameof(SendEmailConsumer)} Execution started");

                if (context.Message.EventType.Equals(MdpEvent.SingleAuthRegistration))
                {
                    var result = await SendRegistrationEmail(context.Message);
                    if (result.IsLeft)
                    {
                        throw new MdpConsumerException(result.Left().Message);
                    }

                    var pdf = await _pdfGenerator.Generate(result.Right().html);

                    await _publisher.Publish(new EdmsUpload()
                    {
                        Bgroup = result.Right().bgroup,
                        Refno = result.Right().refNo,
                        EventType = MdpEvent.SingleAuthRegistration,
                        File = pdf,
                    }, context.CorrelationId);
                }
                else
                {
                    _logger.LogError("{methodName} Invalid event type - {eventType}", nameof(SendEmailConsumer), context.Message.EventType);
                }

                _logger.LogInformation("{methodName} Execution successful", nameof(SendEmailConsumer));
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "{methodName} {error}", nameof(SendEmailConsumer), ex.Message);
                throw;
            }
        }
    }

    public async Task<Either<Error, (string html, string bgroup, string refNo)>> SendRegistrationEmail(EmailNotification message)
    {
        var result = await _singleAuthService.GetLinkedRecord(message.MemberAuthGuid.Value, message.Bgroup);
        var hasLinkedRecord = result.Any();

        var member = await _memberRepository.FindMember(message.Refno, message.Bgroup);

        if (member.IsSome)
        {
            var data = new
            {
                tenantUrl = message.ContentAccessKey,
                wordingFlags = new List<string> { $"scheme_{member.Value().SchemeCode}" }
            };

            var template = await _contentClient.FindTemplate(message.TemplateName, JsonSerializer.Serialize(data));

            var html = await _registrationEmailTemplate.RenderHtml(template.HtmlBody, member.Value().PersonalDetails.Forenames, hasLinkedRecord);

            await _smtpClient.Send(message.To, template.EmailFrom, html, template.EmailSubject);
            _logger.LogInformation("{methodName} Email sent", nameof(SendRegistrationEmail));

            return (html, message.Bgroup, message.Refno);
        }
        else
        {
            return Error.New("Member data not found");
        }
    }
}
