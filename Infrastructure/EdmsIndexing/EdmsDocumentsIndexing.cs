using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Infrastructure.Compressions;
using WTW.MdpService.Infrastructure.Edms;
using WTW.MdpService.Infrastructure.Gbg;
using WTW.Web.Extensions;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Infrastructure.EdmsIndexing;

public class EdmsDocumentsIndexing : IEdmsDocumentsIndexing
{
    private readonly IEdmsClient _edmsClient;
    private readonly ICachedGbgClient _gbgClient;
    private readonly ICachedGbgAdminClient _gbgAdminClient;
    private readonly ILogger<EdmsDocumentsIndexing> _logger;
    private readonly Regex _idScanPdfFileNamePattern = new(@"_\d+-\d+\.pdf$");

    public EdmsDocumentsIndexing(
        IEdmsClient edmsClient,
        ICachedGbgClient gbgClient,
        ICachedGbgAdminClient gbgAdminClient,
        ILogger<EdmsDocumentsIndexing> logger)
    {
        _edmsClient = edmsClient;
        _gbgClient = gbgClient;
        _gbgAdminClient = gbgAdminClient;
        _logger = logger;
    }

    public async Task<Either<Error, List<(int ImageId, string DocUuid)>>> PostIndexTransferDocuments(
        string businessGroup,
        string referenceNumber,
        string caseNumber,
        TransferJourney journey,
        IList<UploadedDocument> documents)
    {
        var postindexResult = await _edmsClient.PostindexDocuments(
            businessGroup,
            referenceNumber,
            caseNumber,
            documents);
        if (postindexResult.IsLeft)
        {
            _logger.LogWarning($"Case number: {caseNumber}. Failed to postindex documents: Documents ids:" +
                $" {string.Join(',', documents.Select(d => d.Uuid))}. Error: {postindexResult.Left().GetErrorMessage()}.");
            return Error.New(postindexResult.Left().GetErrorMessage());
        }

        var unsuccessfullyIndexedDocuments = postindexResult.Right().Documents.Where(d => !d.Indexed);
        if (unsuccessfullyIndexedDocuments.Any())
        {
            var message = $"Case number: {caseNumber}. Failed to postindex documents: Documents ids:" +
                $" {string.Join(';', unsuccessfullyIndexedDocuments.Select(d => d.DocUuid))}." +
                $"Error: {string.Join(';', unsuccessfullyIndexedDocuments.Select(d => d.Message))}.";
            _logger.LogWarning(message);
            return Error.New(message);
        }

        return postindexResult.Right().Documents.Select(x => (x.ImageId, x.DocUuid)).ToList();
    }

    public async Task<Either<Error, EdmsPreIndexResult>> PreIndexRetirement(
    RetirementJourney journey,
    string referenceNumber,
    string businessGroup,
    MemoryStream summaryPdf)
    {
        var retirementApplicationResult = await _edmsClient.PreindexDocument(
            businessGroup,
            referenceNumber,
            $"{businessGroup}1",//todo remove hardcoded value
            summaryPdf);
        if (!retirementApplicationResult.IsRight)
            return Error.New(retirementApplicationResult.Left().Message);

        _logger.LogInformation($"EDMS pre index batch number {retirementApplicationResult.Right().BatchNumber}");

        if (journey.GbgId.HasValue)
        {
            var zip = await _gbgClient.GetDocuments(new List<Guid> { journey.GbgId.Value }).Try();
            if (!zip.IsSuccess)
            {
                var error = $"Failed to get gbg document: {zip}";
                _logger.LogError(error);
                return Error.New(error);
            }

            var pdf = await FileCompression.Unzip(zip.Value(), FileFilter.Pdf).SingleOrDefaultAsync();
            if (pdf == null)
                return Error.New("Identity document not found");

            await zip.Value().DisposeAsync();

            var newPdfName = GetGbgPdfFileName(pdf.Name);
            var renamedPdfFileStream = await pdf.Stream.RenameFile(newPdfName);

            var identityDocumentResult = await _edmsClient.PreindexDocument(
                businessGroup,
                referenceNumber,
                $"{businessGroup}1", //TODO: remove hardcoded value
                renamedPdfFileStream,
                retirementApplicationResult.Right().BatchNumber);

            if (!identityDocumentResult.IsRight)
                return Error.New(identityDocumentResult.Left().Message);

            _logger.LogInformation($"GBG OK {retirementApplicationResult.Right().BatchNumber}");
        }

        return new EdmsPreIndexResult(
            retirementApplicationResult.Right().BatchNumber,
            retirementApplicationResult.Right().ImageId);
    }

    public async Task<Error?> PostIndexRetirement(
        string referenceNumber,
        string businessGroup,
        string caseNumber,
        string caseCode,
        int preIndexBatchNumber)
    {
        var indexResult = await _edmsClient.IndexRetirementDocument(
            businessGroup,
            referenceNumber,
            preIndexBatchNumber,
            caseNumber,
            caseCode);
        if (indexResult.IsLeft)
        {
            _logger.LogError($"Case number: {caseNumber}. Failed to post index retirement: Pre Index Batch Number: {preIndexBatchNumber}." +
             $" {indexResult.Left().GetErrorMessage()}.");
            return Error.New(indexResult.Left().GetErrorMessage());
        }

        return null;
    }

    public async Task<Error?> PostIndexBereavement(
        string businessGroup,
        string caseNumber,
        int preIndexBatchNumber)
    {
        var indexResult = await _edmsClient.IndexBereavementDocument(
            businessGroup,
            preIndexBatchNumber,
            caseNumber);
        if (indexResult.IsLeft)
        {
            _logger.LogError($"Case number: {caseNumber}. Failed to post index bereavement: Pre Index Batch Number: {preIndexBatchNumber}." +
             $" {indexResult.Left().GetErrorMessage()}.");
            return Error.New(indexResult.Left().GetErrorMessage());
        }

        return null;
    }

    public async Task<Either<Error, List<(int imageId, string docUuid)>>> PostIndexBereavementDocuments(
        string businessGroup,
        string caseNumber,
        IList<UploadedDocument> documents)
    {
        var postindexResult = await _edmsClient.PostIndexBereavementDocuments(
            businessGroup,
            caseNumber,
            documents);
        if (postindexResult.IsLeft)
        {
            _logger.LogWarning($"Case number: {caseNumber}. Failed to postindex documents: Documents ids:" +
                $" {string.Join(',', documents.Select(d => d.Uuid))}. Error: {postindexResult.Left().GetErrorMessage()}.");
            return Error.New(postindexResult.Left().GetErrorMessage());
        }

        var unsuccessfullyIndexedDocuments = postindexResult.Right().Documents.Where(d => !d.Indexed);
        if (unsuccessfullyIndexedDocuments.Any())
        {
            var message = $"Case number: {caseNumber}. Failed to postindex documents: Documents ids:" +
                $" {string.Join(';', unsuccessfullyIndexedDocuments.Select(d => d.DocUuid))}." +
                $"Error: {string.Join(';', unsuccessfullyIndexedDocuments.Select(d => d.Message))}.";
            _logger.LogWarning(message);
            return Error.New(message);
        }

        return postindexResult.Right().Documents.Select(x => (x.ImageId, x.DocUuid)).ToList();
    }

    public async Task<Either<Error, HttpStatusCode>> CleanAfterPostIndex(Guid gbgId)
    {
        var clean = await _gbgAdminClient.DeleteJourneyPerson(gbgId.ToString()).Try();
        if (!clean.IsSuccess)
        {
            var error = $"Failed to delete journey person: {clean}";
            _logger.LogError(error);
            return Error.New(error);
        }

        return clean.Value();
    }

    private string GetGbgPdfFileName(string name) => $"GBG{_idScanPdfFileNamePattern.Match(name).Value}";
}