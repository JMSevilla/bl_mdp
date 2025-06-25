using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Infrastructure.Edms;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Infrastructure.Journeys.Documents;

public class JourneyDocumentsHandlerService : IJourneyDocumentsHandlerService
{
    private readonly IJourneyDocumentsRepository _journeyDocumentsRepository;
    private readonly IEdmsClient _edmsClient;
    private readonly ILogger<JourneyDocumentsHandlerService> _logger;

    public JourneyDocumentsHandlerService(
        IJourneyDocumentsRepository journeyDocumentsRepository,
        IEdmsClient edmsClient,
        ILogger<JourneyDocumentsHandlerService> logger)
    {
        _journeyDocumentsRepository = journeyDocumentsRepository;
        _edmsClient = edmsClient;
        _logger = logger;
    }

    public async Task<Either<Error, Unit>> PostIndex(string businessGroup, string referenceNumber, string caseNumber, string journeyType)
    {
        if (string.IsNullOrEmpty(caseNumber))
        {
            _logger.LogWarning("{journeyType} case number is null or empty for reference number {businessGroup}:{referenceNumber}", journeyType, businessGroup, referenceNumber);
            return Error.New($"{journeyType} case must be sumbitted.");
        }

        var documents = await _journeyDocumentsRepository.List(businessGroup, referenceNumber, journeyType);
        if (!documents.Any())
        {
            _logger.LogWarning("Cannot find any documents to postindex for case number {caseNumber}", caseNumber);
            return Error.New("Cannot find any documents to postindex.");
        }

        var postindexResult = await _edmsClient.PostindexDocuments(
                        businessGroup,
                        referenceNumber,
                        caseNumber,
                        documents.ToList());

        if (postindexResult.IsLeft)
        {
            _logger.LogWarning("Case number: {caseNumber}. Failed to postindex documents: Documents ids: {Uuids}. Error: {Message}.",
                caseNumber, string.Join(',', documents.Select(d => d.Uuid)), postindexResult.Left().GetErrorMessage());

            return Error.New(postindexResult.Left().GetErrorMessage());
        }

        _journeyDocumentsRepository.RemoveAll(documents);
        return Unit.Default;
    }
}