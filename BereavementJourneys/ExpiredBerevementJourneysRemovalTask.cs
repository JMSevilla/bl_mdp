using System;
using System.Threading;
using System.Threading.Tasks;
using WTW.MdpService.BackgroundTasks;
using WTW.MdpService.Infrastructure.BereavementDb;
using WTW.MdpService.Infrastructure.MdpDb;

namespace WTW.MdpService.BereavementJourneys;

public class ExpiredBerevementJourneysRemovalTask : IBackgroundTask
{
    private readonly BereavementJourneyRepository _bereavementJourneyRepository;
    private readonly IBereavementContactConfirmationRepository _bereavementContactConfirmationRepository;
    private readonly BereavementJourneyConfiguration _journeyConfiguration;
    private readonly BereavementUnitOfWork _bereavementUnitOfWork;
    private DateTimeOffset? _nextRemovalDate;

    public ExpiredBerevementJourneysRemovalTask(BereavementJourneyRepository bereavementJourneyRepository,
        IBereavementContactConfirmationRepository bereavementContactConfirmationRepository,
        BereavementJourneyConfiguration journeyConfiguration,
        BereavementUnitOfWork bereavementUnitOfWork)
    {
        _bereavementJourneyRepository = bereavementJourneyRepository;
        _bereavementContactConfirmationRepository = bereavementContactConfirmationRepository;
        _journeyConfiguration = journeyConfiguration;
        _bereavementUnitOfWork = bereavementUnitOfWork;
    }

    public async Task Start(CancellationToken stoppingToken)
    {
        _nextRemovalDate = _nextRemovalDate ?? DateTimeOffset.UtcNow;

        if (_nextRemovalDate.Value < DateTimeOffset.UtcNow)
        {
            await using var transaction = await _bereavementUnitOfWork.BeginTransactionAsync();
            var journeys = await _bereavementJourneyRepository.FindExpired(DateTimeOffset.UtcNow);
            var confirmations = await _bereavementContactConfirmationRepository.FindExpiredUnlocked(DateTimeOffset.UtcNow, _journeyConfiguration.EmailLockPeriodInMin);
            _bereavementJourneyRepository.Remove(journeys);
            _bereavementContactConfirmationRepository.Remove(confirmations);
            await _bereavementUnitOfWork.Commit();
            await transaction.CommitAsync(stoppingToken);
            _nextRemovalDate = DateTimeOffset.UtcNow.AddMinutes(_journeyConfiguration.ExpiredJourneysRemovalPeriodInMin);
        }
    }
}