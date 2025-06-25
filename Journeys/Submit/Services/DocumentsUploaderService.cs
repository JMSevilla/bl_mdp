using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Common.UploadedDocuments;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Edms;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.MdpService.Infrastructure.MemberDb.Documents;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Journeys.Submit.Services;

public class DocumentsUploaderService : IDocumentsUploaderService
{
    private const string Retirement = "retirement";
    private readonly IEdmsClient _edmsClient;
    private readonly IDocumentFactoryProvider _documentFactoryProvider;
    private readonly IDocumentsRepository _documentsRepository;
    private readonly IMemberDbUnitOfWork _memberUnitOfWork;
    private readonly IJourneyDocumentsRepository _journeyDocumentsRepository;
    private readonly IUploadedDocumentFactory _uploadedDocumentFactory;
    private readonly ICalculationHistoryRepository _calculationHistoryRepository;
    private readonly ILogger<DocumentsUploaderService> _logger;

    public DocumentsUploaderService(
        IEdmsClient edmsClient,
        IDocumentFactoryProvider documentFactoryProvider,
        IDocumentsRepository documentsRepository,
        IMemberDbUnitOfWork memberUnitOfWork,
        IJourneyDocumentsRepository journeyDocumentsRepository,
        IUploadedDocumentFactory uploadedDocumentFactory,
        ICalculationHistoryRepository calculationHistoryRepository,
        ILogger<DocumentsUploaderService> logger)
    {
        _edmsClient = edmsClient;
        _documentFactoryProvider = documentFactoryProvider;
        _documentsRepository = documentsRepository;
        _memberUnitOfWork = memberUnitOfWork;
        _journeyDocumentsRepository = journeyDocumentsRepository;
        _uploadedDocumentFactory = uploadedDocumentFactory;
        _calculationHistoryRepository = calculationHistoryRepository;
        _logger = logger;
    }

    public async Task<Either<Error, int>> UploadGenericRetirementDocuments(string businessGroup, string referenceNumber, string caseNumber, string journeyType, MemoryStream pdfStream, string fileName)
    {
        var edmsResultForSummary = await UploadDocumentToEdms(businessGroup, pdfStream, fileName);
        if (edmsResultForSummary.IsLeft)
            return Error.New($"Generic Retirement documents upload process failed. Journey Type: {journeyType}.");

        var journeyDocuments = await _journeyDocumentsRepository.List(businessGroup, referenceNumber, journeyType);
        journeyDocuments.Add(_uploadedDocumentFactory.CreateOutgoing(referenceNumber, businessGroup, fileName, edmsResultForSummary.Right().Uuid, true, "MDPRETAPP"));

        var errorOrPostIndexDocumentsDetails = await PostIndexDocuments(businessGroup, referenceNumber, caseNumber, journeyDocuments);
        if (errorOrPostIndexDocumentsDetails.IsLeft)
            return errorOrPostIndexDocumentsDetails.Left();

        var imageId = errorOrPostIndexDocumentsDetails.Right().Single(x => x.DocUuid == edmsResultForSummary.Right().Uuid).ImageId;
        await SaveDocumentRecord(businessGroup, referenceNumber, caseNumber, GetDocumentType(journeyType, null), imageId);
        return imageId;
    }

    public async Task<Either<Error, int>> UploadDBRetirementDocument(string businessGroup, string referenceNumber, string caseNumber, MemoryStream pdfStream, string fileName, Guid? gbgId = null)
    {
        string journeyType = "dbretirementapplication";
        var edmsResultForSummary = await UploadDocumentToEdms(businessGroup, pdfStream, fileName);
        if (edmsResultForSummary.IsLeft)
            return Error.New($"DB Retirement documents upload process failed.");

        var journeyDocuments = new List<UploadedDocument>();
        journeyDocuments.Add(_uploadedDocumentFactory.CreateOutgoing(referenceNumber, businessGroup, fileName, edmsResultForSummary.Right().Uuid, true, "MDPRETAPP"));

        var errorOrPostIndexDocumentsDetails = await PostIndexDocuments(businessGroup, referenceNumber, caseNumber, journeyDocuments);
        if (errorOrPostIndexDocumentsDetails.IsLeft)
            return errorOrPostIndexDocumentsDetails.Left();

        var imageId = errorOrPostIndexDocumentsDetails.Right().Single(x => x.DocUuid == edmsResultForSummary.Right().Uuid).ImageId;
        await SaveDocumentRecord(businessGroup, referenceNumber, caseNumber, GetDocumentType(journeyType, null), imageId);

        return imageId;
    }

    public async Task<Error?> UploadQuoteRequestSummary(string businessGroup, string referenceNumber, string caseNumber, string caseType, MemoryStream summaryPdf, string fileName)
    {
        var edmsResult = await UploadDocumentToEdms(businessGroup, summaryPdf, fileName);
        if (edmsResult.IsLeft)
            return Error.New("Quote request documents upload process failed.");

        var documentTag = caseType.Equals(Retirement, StringComparison.InvariantCultureIgnoreCase) ? "RETQU_SUM" : "TRNQU_SUM";
        var document = _uploadedDocumentFactory.CreateOutgoing(referenceNumber, businessGroup, fileName, edmsResult.Right().Uuid, true, documentTag);

        var errorOrPostIndexDocumentsDetails = await PostIndexDocuments(businessGroup, referenceNumber, caseNumber, new List<UploadedDocument> { document });
        if (errorOrPostIndexDocumentsDetails.IsLeft)
            return errorOrPostIndexDocumentsDetails.Left();

        await SaveDocumentRecord(businessGroup, referenceNumber, caseNumber, GetDocumentType(null, caseType), errorOrPostIndexDocumentsDetails.Right().Single().ImageId);
        return null;
    }

    public async Task UploadNonCaseRetirementQuoteDocument(string businessGroup, string referenceNumber, MemoryStream memoryStream, int calcSystemHistorySeqno)
    {
        var latestCalcHistory = await _calculationHistoryRepository.FindLatest(businessGroup, referenceNumber, calcSystemHistorySeqno);

        if (latestCalcHistory.IsSome && !(latestCalcHistory.Value().ImageId.HasValue))
        {
            var documentName = "Retirement quote";
            var edmsResult = await UploadDocumentToEdms(businessGroup, memoryStream, documentName);
            var document = _uploadedDocumentFactory.CreateIncoming(referenceNumber, businessGroup, documentName, edmsResult.Right().Uuid, false, "RETQU");

            var postindexResult = await _edmsClient.IndexNonCaseDocuments(
              businessGroup,
              referenceNumber,
              new List<UploadedDocument> { document });
            if (postindexResult.IsLeft)
                _logger.LogError("Failed to post index document: Documents id: {documentId}. Erros: {eroors}.",
                     edmsResult.Right().Uuid, postindexResult.Left().GetErrorMessage());

            await SaveDocumentRecord(businessGroup, referenceNumber, null, DocumentType.RetirementQuoteWithoutCase, postindexResult.Right().Documents.Single().ImageId);

            await (latestCalcHistory)
                .IfSomeAsync(async x =>
                {
                    x.UpdateIds(postindexResult.Right().Documents.Single().ImageId, null);
                    _logger.LogInformation("Updating cacculation history. ImageId: {imageId}.", postindexResult.Right().Documents.Single().ImageId);
                    await _memberUnitOfWork.Commit();
                });
        }
    }

    private static DocumentType GetDocumentType(string journeyType, string quoteRequestCaseType)
    {
        return (journeyType?.ToLower(), quoteRequestCaseType?.ToLower()) switch
        {
            (null, Retirement) => DocumentType.RetirementQuoteRequest,
            (null, "transfer") => DocumentType.TransferQuoteRequest,
            ("dcretirementapplication", null) => DocumentType.DcRetirement,
            ("dbcoreretirementapplication", null) => DocumentType.Retirement,
            ("dbretirementapplication", null) => DocumentType.Retirement,
            _ => throw new ArgumentException($"Unsupported Journey Type or Quote Request Case Type. Journey Type: {journeyType ?? "null"}. Quote Request Case Type: {quoteRequestCaseType ?? "null"}. ")
        };
    }

    private async Task SaveDocumentRecord(string businessGroup, string referenceNumber, string caseNumber, DocumentType documentType, int imageId)
    {
        var summaryDocument = _documentFactoryProvider.GetFactory(documentType).Create(
                              businessGroup,
                              referenceNumber,
                              await _documentsRepository.NextId(),
                              imageId,
                              DateTimeOffset.UtcNow,
                              caseNumber);

        _documentsRepository.Add(summaryDocument);
        _logger.LogInformation("Saving documents to database. ImageId: {imageId}.", imageId);
        await _memberUnitOfWork.Commit();
    }

    private async Task<Either<DocumentUploadError, DocumentUploadResponse>> UploadDocumentToEdms(string businessGroup, MemoryStream fileSteam, string fileName)
    {
        var response = await _edmsClient.UploadDocument(businessGroup, fileName, fileSteam);
        if (response.IsLeft)
            _logger.LogError("Failed to upload summary file. File name: \"{fileName}\". Error: {message}", fileName, response.Left().Message);

        return response;
    }

    private async Task<Either<Error, List<(int ImageId, string DocUuid)>>> PostIndexDocuments(
        string businessGroup,
        string referenceNumber,
        string caseNumber,
        IList<UploadedDocument> documents)
    {
        var postindexResult = await _edmsClient.PostindexDocuments(
            businessGroup,
            referenceNumber,
            caseNumber,
            documents);
        if (postindexResult.IsLeft)
        {
            _logger.LogError($"Case number: {caseNumber}. Failed to post index documents: Documents ids: {string.Join(',', documents.Select(d => d.Uuid))}." +
                $" {postindexResult.Left().GetErrorMessage()}.");
            return Error.New("Failed to post index documents.");
        }

        var unsuccessfullyIndexedDocuments = postindexResult.Right().Documents.Where(d => !d.Indexed);
        if (unsuccessfullyIndexedDocuments.Any())
        {
            var message = $"Case number: {caseNumber}. Failed to post index documents: Documents ids:" +
                $" {string.Join(';', unsuccessfullyIndexedDocuments.Select(d => d.DocUuid))}." +
                $"Error: {string.Join(';', unsuccessfullyIndexedDocuments.Select(d => d.Message))}.";
            _logger.LogError(message);
            return Error.New(message);
        }

        return postindexResult.Right().Documents.Select(x => (x.ImageId, x.DocUuid)).ToList();
    }
}