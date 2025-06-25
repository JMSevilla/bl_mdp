using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Infrastructure.Compressions;
using WTW.MdpService.Infrastructure.Gbg;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.Web.Extensions;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Journeys.Submit.Services;

public class GbgDocumentService : IGbgDocumentService
{
    private readonly Regex _idScanPdfFileNamePattern = new(@"_\d+-\d+\.pdf$");
    private readonly IJourneysRepository _journeysRepository;
    private readonly ICachedGbgClient _gbgClient;
    private readonly ILogger<GbgDocumentService> _logger;

    public GbgDocumentService(IJourneysRepository journeysRepository, ICachedGbgClient gbgClient, ILogger<GbgDocumentService> logger)
    {
        _journeysRepository = journeysRepository;
        _gbgClient = gbgClient;
        _logger = logger;
    }

    public async Task<Either<Error, (MemoryStream DocumentStream, string FileName)>> GetGbgFile(string businessGroup, string referenceNumber, string journeyType, string caseNumber)
    {
        var journey = await _journeysRepository.Find(businessGroup, referenceNumber, journeyType);
        if (journey.IsNone)
        {
            _logger.LogError("Failed to retrieve journey with type {journeyType}.Case number: {caseNumber}.", journeyType, caseNumber);
            return Error.New($"Failed to retrieve journey with type {journeyType}.");
        }

        var gbgJsonString = journey.Value().GetGenericDataByFormKey("gbg_user_identification_form_id")?.GenericDataJson;
        var gbgIdOrError = gbgJsonString.GetValueFromJson("gbgId");
        if (gbgIdOrError.IsLeft)
        {
            _logger.LogError("Failed to retrieve gbg id with journey type {journeyType}.Case number: {caseNumber}. Error: {error}", journeyType, caseNumber, gbgIdOrError.Left());
            return Error.New($"Failed to retrieve gbg id. Journey type: {journeyType}.");
        }

        if(!Guid.TryParse(gbgIdOrError.Right(), out var gbgId))
        {
            _logger.LogError("Failed to parse {gbgId} with journey type {journeyType}", journeyType, gbgIdOrError.Right());
            return Error.New($"Failed to retrieve gbg id. Journey type: {journeyType}.");
        }
        
        var zip = await _gbgClient.GetDocuments(new List<Guid> { gbgId }).Try();
        if (zip.IsFaulted)
        {
            zip.IfFail(e => _logger.LogError(e, "Failed to retrieve gbg document. Case number: {caseNumber}.", caseNumber));
            return Error.New("Failed to retrieve gbg document");
        }

        InMemoryFile pdf;
        await using (var zipStream = zip.Value())
        {
            pdf = await FileCompression.Unzip(zipStream, FileFilter.Pdf).SingleOrDefaultAsync();
            if (pdf == null)
            {
                _logger.LogWarning("Case number: {caseNumber}. Identity document not found", caseNumber);
                return Error.New($"Case number: {caseNumber}. Identity document not found");
            }
        }

        return await RenameGbgFile(pdf);
    }

    public async Task<Either<Error, (MemoryStream DocumentStream, string FileName)>> GetGbgFile(Guid gbgId)
    {
        var zip = await _gbgClient.GetDocuments(new List<Guid> { gbgId }).Try();
        if (!zip.IsSuccess)
        {
            var error = "Failed to get gbg document.";
            _logger.LogError(error);
            return Error.New(error);
        }

        var pdf = await FileCompression.Unzip(zip.Value(), FileFilter.Pdf).SingleOrDefaultAsync();
        if (pdf == null)
            return Error.New("Identity document not found.");

        await zip.Value().DisposeAsync();

        return await RenameGbgFile(pdf);
    }

    private string GetGbgPdfFileName(string name) => $"GBG{_idScanPdfFileNamePattern.Match(name).Value}";

    private async Task<(MemoryStream DocumentStream, string FileName)> RenameGbgFile(InMemoryFile pdf)
    {
        var newPdfName = GetGbgPdfFileName(pdf.Name);
        var renamedPdfFileStream = await pdf.Stream.RenameFile(newPdfName);
        return (renamedPdfFileStream, newPdfName);
    }
}