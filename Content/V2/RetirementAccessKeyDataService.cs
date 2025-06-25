using System;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Content.V2;

public class RetirementAccessKeyDataService : IRetirementAccessKeyDataService
{
    private const string ForbiddenCalculationStatus = "forbidden";
    private const string NoFiguresCalculationStatus = "noFigures";
    private readonly ICalculationsRepository _calculationsRepository;
    private readonly ICalculationsClient _calculationsClient;
    private readonly IMemberRepository _memberRepository;
    private readonly IRetirementJourneyRepository _retirementJourneyRepository;
    private readonly ICalculationsParser _calculationsParser;
    private readonly IMdpUnitOfWork _mdpUnitOfWork;
    private readonly ILogger<RetirementAccessKeyDataService> _logger;
    private readonly IJourneysRepository _journeysRepository;

    public RetirementAccessKeyDataService(ICalculationsRepository calculationsRepository,
        ICalculationsClient calculationsClient,
        IMemberRepository memberRepository,
        IRetirementJourneyRepository retirementJourneyRepository,
        ICalculationsParser calculationsParser,
        IMdpUnitOfWork mdpUnitOfWork,
        ILogger<RetirementAccessKeyDataService> logger,
        IJourneysRepository journeysRepository)
    {
        _calculationsRepository = calculationsRepository;
        _calculationsClient = calculationsClient;
        _memberRepository = memberRepository;
        _retirementJourneyRepository = retirementJourneyRepository;
        _calculationsParser = calculationsParser;
        _mdpUnitOfWork = mdpUnitOfWork;
        _logger = logger;
        _journeysRepository = journeysRepository;
    }

    public async Task<Either<Error, Calculation>> GetNewRetirementCalculation(RetirementDatesAgesResponse retirementDatesAgesResponse, Member member)
    {
        var now = DateTimeOffset.UtcNow;
        var newCalculation = await NewCalculation(retirementDatesAgesResponse, now, member);
        if (newCalculation.IsRight)
            await SaveNewCalculation(member.ReferenceNumber, member.BusinessGroup, newCalculation.Right());

        if (newCalculation.IsLeft && IsStatusSavable(newCalculation.Left()))
        {
            var calculation = await SaveForbiddenCalculation(GetStatusFromError(newCalculation.Left()), member.ReferenceNumber, member.BusinessGroup, now, retirementDatesAgesResponse);
            return calculation;
        }

        return newCalculation;
    }

    public RetirementApplicationStatus GetRetirementApplicationStatus(Member member,
       Either<Error, Calculation> retirementCalculation,
       int preRetirementAgePeriodInYears,
       int newlyRetiredRangeInMonth)
    {
        if (retirementCalculation.IsLeft || retirementCalculation.Right().IsCalculationSuccessful == false)
            return RetirementApplicationStatus.Undefined;

        var retirementDatesAges = _calculationsParser.GetRetirementDatesAges(retirementCalculation.Right().RetirementDatesAgesJson);
        RetirementApplicationStatus status;
        try
        {
            status = member.GetRetirementApplicationStatus(
            DateTimeOffset.UtcNow,
            preRetirementAgePeriodInYears,
            newlyRetiredRangeInMonth,
            retirementCalculation.Right().RetirementJourney != null,
            retirementCalculation.Right().IsRetirementJourneySubmitted(),
            retirementCalculation.Right().HasRetirementJourneyExpired(DateTimeOffset.UtcNow),
            // TODO: remove after front access functionality

            retirementCalculation.Right().RetirementJourney != null && !retirementCalculation.Right().HasRetirementJourneyExpired(DateTimeOffset.UtcNow)
                ? retirementCalculation.Right().RetirementJourney.MemberQuote.SearchedRetirementDate
                : retirementCalculation.Right().EffectiveRetirementDate,
            retirementDatesAges);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Retirement applications status calculation failed. Error {exception}", ex);
            status = RetirementApplicationStatus.Undefined;
        }

        return status;
    }

    public async Task<ExistingRetirementJourneyType> GetExistingRetirementJourneyType(Member member)
    {
        if (member.IsSchemeDc() && (await _calculationsRepository.Find(member.ReferenceNumber, member.BusinessGroup)).IsSome)
        {
            _logger.LogInformation("Retirement applications: {retirementApplication}", ExistingRetirementJourneyType.DcRetirementApplication);
            return ExistingRetirementJourneyType.DcRetirementApplication;
        }

        var persistedCalculationWithJourney = await _calculationsRepository.FindWithJourney(member.ReferenceNumber, member.BusinessGroup);
        if (persistedCalculationWithJourney.IsSome && !await TryRemoveCalculationWithRetirementJourney(member, persistedCalculationWithJourney.Value()))
        {
            _logger.LogInformation("Retirement applications: {retirementApplication}", ExistingRetirementJourneyType.DbRetirementApplication);
            return ExistingRetirementJourneyType.DbRetirementApplication;
        }

        _logger.LogInformation("Retirement applications: {retirementApplication}", ExistingRetirementJourneyType.None);
        return ExistingRetirementJourneyType.None;
    }

    public async Task<Either<Error, Calculation>> GetRetirementCalculationWithJourney(
        RetirementDatesAgesResponse retirementDatesAgesResponse,
        string referenceNumber,
        string businessGroup)
    {
        var persistedCalculationWithJourney = await _calculationsRepository.FindWithJourney(referenceNumber, businessGroup);
        if (persistedCalculationWithJourney.IsNone)
            throw new InvalidOperationException("Method \"HasRetirementCalculationWithJourney\" must be called before this method.");

        var error = await UpdateCalculationData(retirementDatesAgesResponse, persistedCalculationWithJourney.Value(), referenceNumber, businessGroup, DateTimeOffset.UtcNow);

        return error.HasValue ? error.Value : persistedCalculationWithJourney.Value();
    }

    public async Task<Option<Calculation>> GetRetirementCalculation(string referenceNumber, string businessGroup)
    {
        return await _calculationsRepository.Find(referenceNumber, businessGroup);
    }

    public async Task UpdateRetirementDatesAges(Calculation calculation, RetirementDatesAgesResponse retirementDatesAgesResponse)
    {
        var retirementDatesAgesJson = _calculationsParser.GetRetirementDatesAgesJson(retirementDatesAgesResponse);
        calculation.UpdateRetirementDatesAgesJson(retirementDatesAgesJson);
        await _mdpUnitOfWork.Commit();
    }

    private async Task<Error?> UpdateCalculationData(
        RetirementDatesAgesResponse retirementDatesAgesResponse,
        Calculation persistedCalculationWithJourney,
        string referenceNumber,
        string businessGroup,
        DateTimeOffset utcNow)
    {
        Error? error = null;
        if (IsJourneySubmittedWithRetirementJsonV2Null(persistedCalculationWithJourney))
            await UpdateSubmittedJourneyWithRetirementJsonV2Null(persistedCalculationWithJourney, referenceNumber, businessGroup, utcNow);

        if (IsJourneyUnsubmittedExpired(persistedCalculationWithJourney, utcNow))
            error = await UpdateUnsubmittedExpiredRetirementJourney(retirementDatesAgesResponse, persistedCalculationWithJourney, referenceNumber, businessGroup, utcNow);

        if (persistedCalculationWithJourney.RetirementJourney.IsGbgStepOlderThan30Days(utcNow))
            await RemoveGbgAndFurtherSteps(persistedCalculationWithJourney);

        return error;
    }

    private async Task<bool> TryRemoveCalculationWithRetirementJourney(Member member, Calculation persistedCalculationWithJourney)
    {
        if (member.IsLastRTP9ClosedOrAbandoned() && persistedCalculationWithJourney.RetirementJourney.IsRetirementJourneySubmitted())
        {
            _logger.LogInformation("Removing retirement calculation and retirement journey for member: BusinessGroup: {businessGroup}, ReferenceNumber:{referenceNumber}",
                member.BusinessGroup, member.ReferenceNumber);
            _calculationsRepository.Remove(persistedCalculationWithJourney);
            _retirementJourneyRepository.Remove(persistedCalculationWithJourney.RetirementJourney);
            await _mdpUnitOfWork.Commit();
            return true;
        }

        return false;
    }

    private async Task RemoveGbgAndFurtherSteps(Calculation persistedCalculationWithJourney)
    {
        _logger.LogInformation("Removing gbg and further steps from retirement journey for member:  BusinessGroup: {businessGroup}, ReferenceNumber:{referenceNumber}",
            persistedCalculationWithJourney.BusinessGroup, persistedCalculationWithJourney.ReferenceNumber);
        persistedCalculationWithJourney.RetirementJourney.RemoveInactiveBranches();
        persistedCalculationWithJourney.RetirementJourney.RemoveStepsStartingWith("submit_review");
        await _mdpUnitOfWork.Commit();
    }

    private async Task<Error?> UpdateUnsubmittedExpiredRetirementJourney(
        RetirementDatesAgesResponse retirementDatesAgesResponse,
        Calculation persistedCalculationWithJourney,
        string referenceNumber,
        string businessGroup,
        DateTimeOffset utcNow)
    {
        var retirementDateAges = new RetirementDatesAges(retirementDatesAgesResponse);
        var retirementDatesAgesJson = _calculationsParser.GetRetirementDatesAgesJson(retirementDatesAgesResponse);
        var effectiveRetirementDate = retirementDateAges.EffectiveDate(utcNow, businessGroup).Date;
        persistedCalculationWithJourney.UpdateEffectiveDate(effectiveRetirementDate.ToUniversalTime());
        persistedCalculationWithJourney.UpdateRetirementDatesAgesJson(retirementDatesAgesJson);

        _logger.LogInformation("Checking if member {businessGroup}:{referenceNumber} is valid for retirement calculation.", businessGroup, referenceNumber);
        if (!await _memberRepository.IsMemberValidForRaCalculation(referenceNumber, businessGroup))
        {
            _logger.LogWarning("Retirement calculation is forbidden for member {businessGroup}:{member}", businessGroup, referenceNumber);
            persistedCalculationWithJourney.UpdateCalculationSuccessStatus(false);
            persistedCalculationWithJourney.SetCalculationStatus(ForbiddenCalculationStatus);
            await _mdpUnitOfWork.Commit();
            return Error.New("Calculation is forbidden for member", ForbiddenCalculationStatus);
        }

        var retirementOrError = await _calculationsClient.RetirementCalculationV2(referenceNumber, businessGroup, effectiveRetirementDate, false);
        if (retirementOrError.IsLeft)
        {
            _logger.LogError("Calculation failed for expired journey. Error: {error}", retirementOrError.Left().Message);

            var persistedRetirementV2 = _calculationsParser.GetRetirementV2(persistedCalculationWithJourney.RetirementJsonV2);
            persistedRetirementV2.SetCalculationFailedWordingFlags(retirementOrError.Left());
            var updatedRetirementJsonV2 = _calculationsParser.GetRetirementJsonV2FromRetirementV2(persistedRetirementV2);
            persistedCalculationWithJourney.UpdateRetirementJsonV2(updatedRetirementJsonV2);
            await _mdpUnitOfWork.Commit();

            return retirementOrError.Left();
        }

        persistedCalculationWithJourney.UpdateCalculationSuccessStatus(true);
        await _mdpUnitOfWork.Commit();
        return null;
    }

    private bool IsJourneyUnsubmittedExpired(Calculation persistedCalculationWithJourney, DateTimeOffset utcNow)
    {
        return persistedCalculationWithJourney.RetirementJourney.HasRetirementJourneyExpired(utcNow) && persistedCalculationWithJourney.RetirementJourney.SubmissionDate is null;
    }

    private bool IsJourneySubmittedWithRetirementJsonV2Null(Calculation persistedCalculationWithJourney)
    {
        return persistedCalculationWithJourney.RetirementJourney.SubmissionDate is not null && string.IsNullOrEmpty(persistedCalculationWithJourney.RetirementJsonV2);
    }

    private async Task UpdateSubmittedJourneyWithRetirementJsonV2Null(
        Calculation persistedCalculationWithJourney,
        string referenceNumber,
        string businessGroup,
        DateTimeOffset utcNow)
    {
        var retirementDatesAges = _calculationsParser.GetRetirementDatesAges(persistedCalculationWithJourney.RetirementDatesAgesJson);
        var effectiveRetirementDate = retirementDatesAges.EffectiveDate(utcNow, businessGroup).Date;

        var retirementOrError = await _calculationsClient.RetirementCalculationV2(referenceNumber, businessGroup, effectiveRetirementDate, false);
        var (retirementJsonV2, mdp) = _calculationsParser.GetRetirementJsonV2(retirementOrError.Right().RetirementResponseV2, retirementOrError.Right().EventType);
        persistedCalculationWithJourney.UpdateRetirementJsonV2(retirementJsonV2);

        _logger.LogInformation("Creating retirementJsonV2 value in DB for user: BusinessGroup: {businessGroup}, ReferenceNumber:{referenceNumber}", businessGroup, referenceNumber);

        await _mdpUnitOfWork.Commit();
    }

    private async Task<Calculation> SaveForbiddenCalculation(string calculationStatusCode, string referenceNumber, string businessGroup, DateTimeOffset utcNow, RetirementDatesAgesResponse retirementDatesAgesResponse)
    {
        var persistedCalculation = await _calculationsRepository.Find(referenceNumber, businessGroup);
        if (persistedCalculation.IsSome)
        {
            _logger.LogInformation("Removing retirement calculation. BusinessGroup: {businessGroup}, ReferenceNumber:{referenceNumber}", businessGroup, referenceNumber);
            _calculationsRepository.Remove(persistedCalculation.Value());
        }

        var retirementDatesAges = new RetirementDatesAges(retirementDatesAgesResponse);
        var retirementDatesAgesJson = _calculationsParser.GetRetirementDatesAgesJson(retirementDatesAgesResponse);
        var effectiveDate = retirementDatesAges.EffectiveDate(utcNow, businessGroup).Date;
        var calc = new Calculation(referenceNumber, businessGroup, retirementDatesAgesJson, string.Empty, string.Empty, effectiveDate.ToUniversalTime(), utcNow, false)
            .SetCalculationStatus(calculationStatusCode);

        await _calculationsRepository.Create(calc);
        await _mdpUnitOfWork.Commit();
        return calc;
    }

    private async Task SaveNewCalculation(string referenceNumber, string businessGroup, Calculation newCalculation)
    {
        var persistedCalculation = await _calculationsRepository.Find(referenceNumber, businessGroup);
        if (persistedCalculation.IsSome)
        {
            _logger.LogInformation("Removing retirement calculation. BusinessGroup: {businessGroup}, ReferenceNumber:{referenceNumber}", businessGroup, referenceNumber);
            _calculationsRepository.Remove(persistedCalculation.Value());
        }

        await _calculationsRepository.Create(newCalculation);
        await _mdpUnitOfWork.Commit();
    }

    private async Task<Either<Error, Calculation>> NewCalculation(RetirementDatesAgesResponse retirementDatesAgesResponse, DateTimeOffset utcNow, Member member)
    {
        var businessGroup = member.BusinessGroup;
        var referenceNumber = member.ReferenceNumber;

        _logger.LogInformation("Checking if member {businessGroup}:{referenceNumber} is valid for retirement calculation.", businessGroup, referenceNumber);
        if (!await _memberRepository.IsMemberValidForRaCalculation(referenceNumber, businessGroup))
        {
            _logger.LogWarning("Retirement calculation is forbidden for member {businessGroup}:{member}", businessGroup, referenceNumber);
            return Error.New("Calculation is forbidden for member", ForbiddenCalculationStatus);
        }

        var retirementDatesAges = new RetirementDatesAges(retirementDatesAgesResponse);
        var retirementDatesAgesJson = _calculationsParser.GetRetirementDatesAgesJson(retirementDatesAgesResponse);
        var effectiveDate = retirementDatesAges.EffectiveDate(utcNow, businessGroup).Date;

        if (member.IsSchemeDc())
        {
            effectiveDate = member.DcRetirementDate(utcNow);
            return new Calculation(referenceNumber, businessGroup, retirementDatesAgesJson, string.Empty, string.Empty, effectiveDate.ToUniversalTime(), utcNow, null);
        }

        var retirementOrError = await _calculationsClient.RetirementCalculationV2(referenceNumber, businessGroup, effectiveDate, false);

        if (retirementOrError.IsLeft)
            return retirementOrError.Left();

        var (retirementJsonV2, mdp) = _calculationsParser.GetRetirementJsonV2(retirementOrError.Right().RetirementResponseV2, retirementOrError.Right().EventType);
        return new Calculation(referenceNumber, businessGroup, retirementDatesAgesJson, retirementJsonV2, mdp, effectiveDate.ToUniversalTime(), utcNow, true);
    }

    private bool IsStatusSavable(Error calculationError)
    {
        return calculationError.Inner.IsSome &&
               (calculationError.Inner.Value().Message == ForbiddenCalculationStatus ||
               calculationError.Inner.Value().Message == NoFiguresCalculationStatus);
    }

    private string GetStatusFromError(Error calculationError)
    {
        return calculationError.Inner.Value().Message;
    }
}