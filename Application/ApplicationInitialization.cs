using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.Redis;
using WTW.MdpService.Journeys.Submit.Services;
using WTW.MdpService.TransferJourneys;
using WTW.Web.Caching;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Application;

public class ApplicationInitialization : IApplicationInitialization
{
    private readonly ICalculationsRedisCache _calculationsRedisCache;
    private readonly ITransferCalculationRepository _transferCalculationRepository;
    private readonly ITransferJourneyRepository _transferJourneyRepository;
    private readonly ITransferOutsideAssure _transferOutsideAssure;
    private readonly ICalculationsClient _calculationsClient;
    private readonly IJourneyDocumentsRepository _journeyDocumentsRepository;
    private readonly IJourneysRepository _journeysRepository;
    private readonly ILogger<ApplicationInitialization> _logger;
    private readonly IMdpUnitOfWork _mdpUnitOfWork;
    private readonly ICache _cache;
    private readonly IGenericJourneyService _genericJourneyService;
    private readonly ICalculationsRepository _calculationsRepository;

    public ApplicationInitialization(ICalculationsRedisCache calculationsRedisCache,
        ITransferCalculationRepository transferCalculationRepository,
        ITransferJourneyRepository transferJourneyRepository,
        ITransferOutsideAssure transferOutsideAssure,
        ICalculationsClient calculationsClient,
        IJourneyDocumentsRepository journeyDocumentsRepository,
        IJourneysRepository journeysRepository,
        ILogger<ApplicationInitialization> logger,
        IMdpUnitOfWork mdpUnitOfWork,
        ICache cache,
        IGenericJourneyService genericJourneyService,
        ICalculationsRepository calculationsRepository)
    {
        _calculationsRedisCache = calculationsRedisCache;
        _transferCalculationRepository = transferCalculationRepository;
        _transferJourneyRepository = transferJourneyRepository;
        _transferOutsideAssure = transferOutsideAssure;
        _calculationsClient = calculationsClient;
        _journeyDocumentsRepository = journeyDocumentsRepository;
        _journeysRepository = journeysRepository;
        _logger = logger;
        _mdpUnitOfWork = mdpUnitOfWork;
        _cache = cache;
        _genericJourneyService = genericJourneyService;
        _calculationsRepository = calculationsRepository;
    }

    public async Task SetUpTransfer(Member member)
    {
        await _calculationsRedisCache.ClearRetirementDateAges(member.ReferenceNumber, member.BusinessGroup);
        var calculationOption = await _transferCalculationRepository.Find(member.BusinessGroup, member.ReferenceNumber);
        var transferJourney = await _transferJourneyRepository.Find(member.BusinessGroup, member.ReferenceNumber);
        if (transferJourney.IsSome)
        {
            if (transferJourney.Value().IsGbgStepOlderThan30Days(DateTimeOffset.UtcNow))
            {
                transferJourney.Value().RemoveInactiveBranches();
                transferJourney.Value().RemoveStepsStartingWith("t2_review_application");
                _logger.LogInformation("Removing GBG step. Refno: {referenceNumber}. BGroup: {businessGroup}", member.ReferenceNumber, member.BusinessGroup);
            }

            if (member.TransferPaperCase().IsSome && member.IsTransferStatusStatedTaOrSubmitStarted(calculationOption.Value()))
            {
                await RemoveTransferData(calculationOption.Value(), member.ReferenceNumber, member.BusinessGroup, transferJourney.Value());
                await _transferOutsideAssure.CreateTransferForEpa(member);
            }

            var datesAges = await _calculationsClient.RetirementDatesAges(member.ReferenceNumber, member.BusinessGroup).Try();
            if (datesAges.IsSuccess && !datesAges.Value().HasLockedInTransferQuote && member.TransferPaperCase().IsNone)
                await RemoveTransferData(calculationOption.Value(), member.ReferenceNumber, member.BusinessGroup, transferJourney.Value());

            await _mdpUnitOfWork.Commit();
            return;
        }

        if (calculationOption.IsSome && transferJourney.IsNone)
            _transferCalculationRepository.Remove(calculationOption.Value());

        await _transferOutsideAssure.CreateTransferForEpa(member);
        await _mdpUnitOfWork.Commit();
        await _transferOutsideAssure.CreateTransferForLockedQuote(member.ReferenceNumber, member.BusinessGroup);
        await _mdpUnitOfWork.Commit();
    }

    public async Task RemoveGenericJourneys(string referenceNumber, string businessGroup)
    {
        var journeys = await _journeysRepository.FindAllMarkedForRemoval(businessGroup, referenceNumber);
        if (!journeys.Any())
            return;

        _journeysRepository.Remove(journeys);
        _logger.LogInformation("Removing {journeyType} journey. Refno: {referenceNumber}. BGroup: {businessGroup}", string.Join(",", journeys.Select(x => x.Type)), referenceNumber, businessGroup);

        var documents = await _journeyDocumentsRepository.List(businessGroup, referenceNumber, journeys.Select(x => x.Type).ToList());
        RemoveDocuments(documents);

        await _mdpUnitOfWork.Commit();
    }

    public async Task UpdateGenericJourneysStatuses(string referenceNumber, string businessGroup)
    {
        var journeys = await _journeysRepository.FindAllExpiredUnsubmitted(businessGroup, referenceNumber);
        if (!journeys.Any())
            return;

        foreach (var journey in journeys)
        {
            journey.SetExpiredStatus();
            _logger.LogInformation("Updating \'{journeyType}\' journey status to expired. Refno: {referenceNumber}. BGroup: {businessGroup}", journey.Type, referenceNumber, businessGroup);
        }

        await _mdpUnitOfWork.Commit();
    }

    public async Task SetUpDcRetirement(Member member)
    {
        if (!member.IsSchemeDc())
            return;

        await SetUpRetirement(member, JourneyTypes.DcRetirementApplication);
    }

    public async Task SetUpDbRetirement(Member member)
    {
        if (member.IsSchemeDc())
            return;

        await SetUpRetirement(member, JourneyTypes.DbCoreRetirementApplication);
    }

    /// <summary>
    /// Sets up the retirement journey for a member.
    /// This function is for generic type journeys only.
    /// </summary>
    /// <param name="member">The member for whom the retirement journey is being set up.</param>
    /// <param name="journeyType">The type of the journey.</param>
    private async Task SetUpRetirement(Member member, string journeyType)
    {
        _logger.LogInformation("SetUpRetirement starts. JourneyType {journeyType}, Refno: {referenceNumber}, BGroup: {businessGroup}",
            journeyType, member.ReferenceNumber, member.BusinessGroup);

        var calculation = await _calculationsRepository.Find(member.ReferenceNumber, member.BusinessGroup);

        if (calculation.IsSome && calculation.Value().RetirementJourney != null)
        {
            _logger.LogInformation("JourneyType is not a generic type and uses RetirementJourney table, Refno: {referenceNumber}, BGroup: {businessGroup}",
                member.ReferenceNumber, member.BusinessGroup);
            return;
        }

        var journey = await _journeysRepository.Find(member.BusinessGroup, member.ReferenceNumber, journeyType);

        if ((calculation.IsSome && journey.IsNone) || (journey.IsSome && journey.Value().IsExpired(DateTimeOffset.UtcNow) && !journey.Value().SubmissionDate.HasValue))
        {
            _logger.LogInformation("Removing calculation. Refno: {referenceNumber}, BGroup: {businessGroup}",
                member.ReferenceNumber, member.BusinessGroup);

            if (calculation.IsSome)
            {
                _calculationsRepository.Remove(calculation.Value());
                await _mdpUnitOfWork.Commit();
            }
        }

        var detailsOrError = await _genericJourneyService.GetSubmissionDetailsFromGenericData(member.BusinessGroup, member.ReferenceNumber, journeyType);
        if (detailsOrError.IsLeft)
        {
            _logger.LogInformation("Unable to get case number. Reason: {detailsOrError}",
                detailsOrError.Left().Message);
            return;
        }

        if (!member.IsRTP9CaseAbandoned(detailsOrError.Right().CaseNumber))
        {
            _logger.LogInformation("Case is not abandoned. CaseNumber: {detailsOrError}",
                detailsOrError.Right().CaseNumber);
            return;
        }

        _logger.LogInformation("Removing {journeyType} journey. Refno: {referenceNumber}, BGroup: {businessGroup}",
            journeyType, member.ReferenceNumber, member.BusinessGroup);

        _journeysRepository.Remove(journey.Value());
        calculation.IfSome(c => _calculationsRepository.Remove(c));

        var documents = await _journeyDocumentsRepository.List(member.BusinessGroup, member.ReferenceNumber, journeyType);
        RemoveDocuments(documents);

        await _mdpUnitOfWork.Commit();
    }

    private async Task RemoveTransferData(TransferCalculation calc, string referenceNumber, string businessGroup, TransferJourney transferJourney)
    {
        _transferJourneyRepository.Remove(transferJourney);
        _logger.LogInformation("Removing transfer journey. Refno: {referenceNumber}. BGroup: {businessGroup}", referenceNumber, businessGroup);
        _transferCalculationRepository.Remove(calc);
        _logger.LogInformation("Removing transfer calculation. Refno: {referenceNumber}. BGroup: {businessGroup}", referenceNumber, businessGroup);

        var documents = await _journeyDocumentsRepository.List(businessGroup, referenceNumber, "transfer2");
        RemoveDocuments(documents);
    }

    private void RemoveDocuments(IList<UploadedDocument> documents)
    {
        if (documents.Any())
        {
            _logger.LogWarning("Transfer journey uploaded documents will be deleted. Uuids: {uuids}", string.Join(",", documents.Select(x => x.Uuid)));
            _journeyDocumentsRepository.RemoveAll(documents);
        }
    }

    public async Task ClearSessionCache(string referenceNumber, string businessGroup)
    {
        var keyPrefix = $"{CachedPerSessionAttribute.CachedPerSessionKeyPrefix}{businessGroup}-{referenceNumber}";
        _logger.LogInformation("Clearing session cached responses. Key Prefix: {keyPrefix}.", keyPrefix);
        await _cache.RemoveByPrefix(keyPrefix);
    }
}