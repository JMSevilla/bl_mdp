using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Infrastructure.CasesApi;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Journeys.Submit.Services;
using WTW.Web;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Documents;

public class CaseDocumentsService : ICaseDocumentsService
{
    private readonly ITransferJourneyRepository _transferJourneyRepository;
    private readonly IJourneysRepository _journeysRepository;
    private readonly IGenericJourneyService _genericJourneyService;
    private readonly IRetirementJourneyRepository _retirementJourneyRepository;
    private readonly ILogger<CaseDocumentsService> _logger;
    private readonly ICasesClient _casesClient;

    public CaseDocumentsService(
        ITransferJourneyRepository transferJourneyRepository,
        IJourneysRepository journeysRepository,
        ILogger<CaseDocumentsService> logger,
        IGenericJourneyService genericJourneyService,
        IRetirementJourneyRepository retirementJourneyRepository,
        ICasesClient casesClient)
    {
        _transferJourneyRepository = transferJourneyRepository;
        _journeysRepository = journeysRepository;
        _logger = logger;
        _genericJourneyService = genericJourneyService;
        _retirementJourneyRepository = retirementJourneyRepository;
        _casesClient = casesClient;
    }

    public async Task<Either<Error, string>> GetCaseNumber(string businessGroup, string referenceNumber, string caseCode)
    {
        return caseCode.ToLower() switch
        {
            MdpConstants.CaseCodes.TRANSFER => await GetCaseNumberFromTransferJourney(businessGroup, referenceNumber),
            MdpConstants.CaseCodes.RETIREMENT => await GetCaseNumberFromRetirementJourney(businessGroup, referenceNumber),
            MdpConstants.CaseCodes.PaperRetirementApplication => await GetCaseNumberFromPaperRetirement(businessGroup, referenceNumber),
            MdpConstants.CaseCodes.PaperTransferApplication => await GetCaseNumberFromPaperTransfer(businessGroup, referenceNumber),
            _ => await GetCaseNumberFromGenericJourney(businessGroup, referenceNumber, caseCode)
        };
    }

    private async Task<Either<Error, string>> GetCaseNumberFromTransferJourney(string businessGroup, string referenceNumber)
    {
        var transferJourney = await _transferJourneyRepository.Find(businessGroup, referenceNumber);
        if (transferJourney.IsNone)
        {
            _logger.LogWarning("Transfer journey not found for user {businessGroup}-{referenceNumber}.", businessGroup, referenceNumber);
            return Error.New("Transfer journey not found.");
        }

        if (transferJourney.Value().SubmissionDate == null)
        {
            _logger.LogWarning("Transfer journey is not submitted for user {businessGroup}-{referenceNumber}.", businessGroup, referenceNumber);
            return Error.New("Transfer journey is not submitted yet.");
        }

        return transferJourney.Value().CaseNumber;
    }

    private async Task<Either<Error, string>> GetCaseNumberFromRetirementJourney(string businessGroup, string referenceNumber)
    {
        var retirementJourney = await _retirementJourneyRepository.Find(businessGroup, referenceNumber);
        if (retirementJourney.IsNone)
        {
            _logger.LogWarning("Retirement journey not found for user {businessGroup}-{referenceNumber}.", businessGroup, referenceNumber);
            return Error.New("Retirement journey not found.");
        }

        if (retirementJourney.Value().SubmissionDate == null)
        {
            _logger.LogWarning("Retirement journey is not submitted for user {businessGroup}-{referenceNumber}.", businessGroup, referenceNumber);
            return Error.New("Retirement journey is not submitted yet.");
        }

        return retirementJourney.Value().CaseNumber;
    }

    private async Task<Either<Error, string>> GetCaseNumberFromGenericJourney(string businessGroup, string referenceNumber, string caseCode)
    {
        var genericJourney = await _journeysRepository.Find(businessGroup, referenceNumber, caseCode);
        if (genericJourney.IsNone)
        {
            _logger.LogWarning("{journeyType} journey not found for user {businessGroup}-{referenceNumber}.", caseCode, businessGroup, referenceNumber);
            return Error.New($"{caseCode} journey not found.");
        }

        if (genericJourney.Value().SubmissionDate == null)
        {
            _logger.LogWarning("{journeyType} journey is not submitted for user {businessGroup}-{referenceNumber}.", caseCode, businessGroup, referenceNumber);
            return Error.New($"{caseCode} journey is not submitted yet.");
        }

        var detailsOrError = await _genericJourneyService.GetSubmissionDetailsFromGenericData(businessGroup, referenceNumber, caseCode);
        if (detailsOrError.IsLeft)
        {
            _logger.LogError("Failed to get case number from generic data. Error: {error}", detailsOrError.Left().Message);
            return Error.New("Failed to get case number from generic data.");
        }

        return detailsOrError.Right().CaseNumber;
    }

    private async Task<Either<Error, string>> GetCaseNumberFromPaperRetirement(string businessGroup, string referenceNumber)
    {
        var casesOption = await _casesClient.GetRetirementOrTransferCases(businessGroup, referenceNumber);
        if (casesOption.IsNone || !casesOption.Value().Any())
            return Error.New($"Paper retirement case not found.");

        var paperRetirementCase = casesOption.Value().FirstOrDefault(x => x.CaseCode == MdpConstants.CaseCodes.RTP9 && x.CaseStatus == MdpConstants.CaseStatus.Open && x.CaseSource != MdpConstants.AppName);
        if (paperRetirementCase == null)
        {
            _logger.LogWarning("Paper retirement case not found for user {businessGroup}-{referenceNumber}.", businessGroup, referenceNumber);
            return Error.New("Paper retirement case not found.");
        }

        return paperRetirementCase.CaseNumber;
    }

    private async Task<Either<Error, string>> GetCaseNumberFromPaperTransfer(string businessGroup, string referenceNumber)
    {
        var casesOption = await _casesClient.GetRetirementOrTransferCases(businessGroup, referenceNumber);
        if (casesOption.IsNone || !casesOption.Value().Any())
            return Error.New($"Paper transfer case not found.");

        var paperTransferCase = casesOption.Value().FirstOrDefault(x => x.CaseCode == MdpConstants.CaseCodes.TOP9 && x.CaseStatus == MdpConstants.CaseStatus.Open && x.CaseSource != MdpConstants.AppName);
        if (paperTransferCase == null)
        {
            _logger.LogWarning("Paper transfer case not found for user {businessGroup}-{referenceNumber}.", businessGroup, referenceNumber);
            return Error.New("Paper transfer case not found.");
        }

        return paperTransferCase.CaseNumber;
    }
}
