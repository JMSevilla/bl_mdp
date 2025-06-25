using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using MassTransit;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Common.UploadedDocuments;
using WTW.MdpService.Infrastructure.Edms;
using WTW.MessageBroker.Contracts.Mdp;
using WTW.Web;
using WTW.Web.LanguageExt;
using WTW.Web.Logging;

namespace WTW.MdpService.Infrastructure.Consumers;

public class EdmsUploadConsumer : IConsumer<EdmsUpload>
{
    private readonly ILogger<EdmsUploadConsumer> _logger;
    private readonly IEdmsClient _edmsClient;
    private readonly IUploadedDocumentFactory _uploadedDocumentFactory;

    public EdmsUploadConsumer(ILogger<EdmsUploadConsumer> logger, IEdmsClient edmsClient, IUploadedDocumentFactory uploadedDocumentFactory)
    {
        _logger = logger;
        _edmsClient = edmsClient;
        _uploadedDocumentFactory = uploadedDocumentFactory;
    }

    public async Task Consume(ConsumeContext<EdmsUpload> context)
    {
        using (LogsConfiguration.PushProperty(MdpConstants.Bgroup, context.Message.Bgroup))
        using (LogsConfiguration.PushProperty(MdpConstants.Refno, context.Message.Refno))
        using (LogsConfiguration.PushProperty(MdpConstants.CorrelationId, context.CorrelationId.ToString()))
        {
            try
            {

                _logger.LogInformation("{methodName} Execution started", nameof(EdmsUploadConsumer));

                if (context.Message.EventType.Equals(MdpEvent.SingleAuthRegistration))
                {
                    var response = await UploadDocument(context.Message);
                    if (response.IsLeft)
                    {
                        throw new MdpConsumerException(response.LeftAsEnumerable().First().Message);
                    }
                    _logger.LogInformation("{methodName} Execution successful", nameof(EdmsUploadConsumer));
                }
                else
                {
                    _logger.LogError("{methodName} Invalid event type - {eventType}", nameof(EdmsUploadConsumer), context.Message.EventType);
                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "{methodName} {error}", nameof(EdmsUploadConsumer), ex.Message);
                throw;
            }
        }
    }

    public async Task<Either<Error, bool>> UploadDocument(EdmsUpload message)
    {
        var uploadResponse = await _edmsClient.UploadDocumentBase64(message.Bgroup, MdpConstants.SingleAuthRegistrationEmail, message.File);
        if (uploadResponse.IsLeft)
        {
            var error = $"document upload failed with message - {uploadResponse.LeftAsEnumerable().First().Message}";
            return Error.New(error);
        }
        _logger.LogInformation("{methodName} document uploaded successful with id - {Uuid}", nameof(UploadDocument), uploadResponse.Right().Uuid);

        IList<UploadedDocument> documents = new List<UploadedDocument>() {
           _uploadedDocumentFactory.CreateOutgoing(message.Refno, message.Bgroup,MdpConstants.SingleAuthRegistrationEmail, uploadResponse.Right().Uuid,
                                                   false, MdpConstants.SingleAuthRegistrationEmailDocType)
        };

        var docResult = await _edmsClient.IndexNonCaseDocuments(message.Bgroup, message.Refno, documents);
        if (docResult.IsLeft)
        {
            var error = $"document index failed with message - {docResult.Left().Message}";
            return Error.New(error);
        }

        _logger.LogInformation("{methodName} document indexed successful with image id - {imageId}", nameof(UploadDocument), docResult.Right().Documents.First().ImageId);
        return true;
    }
}
