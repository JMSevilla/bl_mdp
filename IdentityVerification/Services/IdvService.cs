using System;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Infrastructure.IdvService;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.Web;
using WTW.Web.Extensions;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.IdentityVerification.Services;

public class IdvService : IIdvService
{
    private readonly IJourneysRepository _journeysRepository;
    private readonly IRetirementJourneyRepository _retirementJourneyRepository;
    private readonly ITransferJourneyRepository _transferJourneyRepository;
    private readonly ILogger<IdvService> _logger;
    private readonly IIdentityVerificationClient _identityVerificationClient;

    public IdvService(
        IJourneysRepository journeysRepository,
        IRetirementJourneyRepository retirementJourneyRepository,
        ITransferJourneyRepository transferJourneyRepository,
        ILogger<IdvService> logger,
        IIdentityVerificationClient identityVerificationClient)
    {
        _journeysRepository = journeysRepository;
        _retirementJourneyRepository = retirementJourneyRepository;
        _transferJourneyRepository = transferJourneyRepository;
        _logger = logger;
        _identityVerificationClient = identityVerificationClient;
    }

    public async Task<Either<Error, UpdateIdentityResultResponse>> SaveIdentityVerification(string businessGroup, string referenceNumber, string caseCode, string caseNumber)
    {
        _logger.LogInformation("Saving identity verification for business group: {businessGroup}, reference number: {referenceNumber}, case code: {caseCode}, case number: {caseNumber}", businessGroup, referenceNumber, caseCode, caseNumber);
        var payload = new SaveIdentityVerificationRequest(caseCode: caseCode, caseNo: caseNumber);

        var response = await _identityVerificationClient.SaveIdentityVerification(businessGroup, referenceNumber, payload);
        if (response == null)
        {
            _logger.LogError("{ActionName} returned non success response.", nameof(SaveIdentityVerification));
            return Error.New("SaveIdentityVerification returned an error.");
        }
        return response;
    }

    public async Task<Either<Error, VerifyIdentityResponse>> VerifyIdentity(string businessGroup, string referenceNumber, string journeyType)
    {
        var gbgJourneyIdOrError = journeyType switch
        {
            JourneyTypes.DbRetirementApplication
                or JourneyTypes.GenericRetirementApplication => await GetRetirementGbgJourneyId(businessGroup, referenceNumber, journeyType),
            JourneyTypes.TransferApplication => await GetTransferGbgJourneyId(businessGroup, referenceNumber, journeyType),
            JourneyTypes.DbCoreRetirementApplication
                or JourneyTypes.DcRetirementApplication =>
                    await GetGenericGbgJourneyId(businessGroup, referenceNumber, journeyType),
            _ => Error.New($"Unsupported journey type: {journeyType}.")
        };

        if (gbgJourneyIdOrError.IsLeft)
        {
            _logger.LogError("Failed to retrieve GBG journey ID for journey type: {journeyType}. Error: {error}", journeyType, gbgJourneyIdOrError.Left());
            return gbgJourneyIdOrError.Left();
        }

        var userId = $"{MdpConstants.AppName}_{businessGroup}{referenceNumber}";
        var requestSource = $"{MdpConstants.AppName}_{journeyType}".Length > 10
            ? $"{MdpConstants.AppName}_{journeyType}".Substring(0, 10)
            : $"{MdpConstants.AppName}_{journeyType}";

        var payload = new VerifyIdentityRequest(journeyId: gbgJourneyIdOrError.Right(),
            userId: userId,
            checkPreviousResult: false,
            requestSource: requestSource,
            overrideLivenessResult: false);

        var response = await _identityVerificationClient.VerifyIdentity(businessGroup, referenceNumber, payload);
        if (response == null)
        {
            _logger.LogError("{ActionName} returned non success response.", nameof(VerifyIdentity));
            return Error.New("VerifyIdentity returned an error.");
        }
        return response;
    }
    private async Task<Either<Error, Guid>> GetRetirementGbgJourneyId(string businessGroup, string referenceNumber, string journeyType)
    {
        var journey = await _retirementJourneyRepository.Find(businessGroup, referenceNumber);
        if (journey.IsNone)
        {
            _logger.LogError("Failed to retrieve journey with type {journeyType}.", journeyType);
            return Error.New($"Failed to retrieve journey with type {journeyType}.");
        }

        var gbgId = journey.Value().GbgId;
        if (!gbgId.HasValue)
        {
            _logger.LogError("Gbg id is null for journey type {journeyType}.", journeyType);
            return Error.New($"Failed to retrieve gbg id. Journey type: {journeyType}.");
        }

        return gbgId.Value;
    }
    private async Task<Either<Error, Guid>> GetTransferGbgJourneyId(string businessGroup, string referenceNumber, string journeyType)
    {
        var journey = await _transferJourneyRepository.Find(businessGroup, referenceNumber);
        if (journey.IsNone)
        {
            _logger.LogError("Failed to retrieve journey with type {journeyType}.", journeyType);
            return Error.New($"Failed to retrieve journey with type {journeyType}.");
        }

        var gbgId = journey.Value().GbgId;
        if (!gbgId.HasValue)
        {
            _logger.LogError("Gbg id is null for journey type {journeyType}.", journeyType);
            return Error.New($"Failed to retrieve gbg id. Journey type: {journeyType}.");
        }

        return gbgId.Value;
    }
    private async Task<Either<Error, Guid>> GetGenericGbgJourneyId(string businessGroup, string referenceNumber, string journeyType)
    {
        var journey = await _journeysRepository.Find(businessGroup, referenceNumber, journeyType);
        if (journey.IsNone)
        {
            _logger.LogError("Failed to retrieve journey with type {journeyType}.", journeyType);
            return Error.New($"Failed to retrieve journey with type {journeyType}.");
        }

        var gbgJsonString = journey.Value().GetGenericDataByFormKey("gbg_user_identification_form_id")?.GenericDataJson;
        var gbgIdOrError = gbgJsonString.GetValueFromJson("gbgId");
        if (gbgIdOrError.IsLeft)
        {
            _logger.LogError("Failed to retrieve gbg id with journey type {journeyType}. Error: {error}", journeyType, gbgIdOrError.Left());
            return Error.New($"Failed to retrieve gbg id. Journey type: {journeyType}.");
        }

        if (!Guid.TryParse(gbgIdOrError.Right(), out var gbgJourneyId))
        {
            _logger.LogError("Failed to parse {gbgJourneyId} with journey type {journeyType}", gbgIdOrError.Right(), journeyType);
            return Error.New($"Failed to retrieve gbg journey id. Journey type: {journeyType}.");
        }

        return gbgJourneyId;
    }
}
