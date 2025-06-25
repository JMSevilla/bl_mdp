using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using WTW.MdpService.BackgroundTasks;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Edms;
using WTW.MdpService.Infrastructure.EdmsIndexing;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.MdpService.Infrastructure.MemberDb.Documents;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.RetirementJourneys;

public class RetirementJourneyPostIndexTask : IBackgroundTask
{
    private readonly EdmsDocumentsIndexing _edmsDocumentsIndexing;
    private readonly MdpUnitOfWork _mdpDbUow;
    private readonly MemberDbUnitOfWork _memberDbUow;
    private readonly PensionWiseRepository _pensionWiseRepository;
    private readonly RetirementPostIndexEventRepository _postIndexEventRepository;
    private readonly RetirementCaseRepository _caseRepository;
    private readonly RetirementJourneyRepository _journeyRepository;
    private readonly IDocumentsRepository _documentsRepository;
    private readonly ILogger<RetirementJourneyPostIndexTask> _logger;
    private readonly EdmsClient _edmsClient;
    private readonly IDocumentFactoryProvider _documentFactoryProvider;

    public RetirementJourneyPostIndexTask(
        EdmsDocumentsIndexing edmsDocumentsIndexing,
        RetirementPostIndexEventRepository postIndexEventRepository,
        RetirementCaseRepository caseRepository,
        RetirementJourneyRepository journeyRepository,
        IDocumentsRepository documentsRepository,
        MdpUnitOfWork mdpDbUow,
        MemberDbUnitOfWork memberDbUow,
        PensionWiseRepository pensionWiseRepository,
        ILogger<RetirementJourneyPostIndexTask> logger,
        EdmsClient edmsClient,
        IDocumentFactoryProvider documentFactoryProvider)
    {
        _edmsDocumentsIndexing = edmsDocumentsIndexing;
        _postIndexEventRepository = postIndexEventRepository;
        _caseRepository = caseRepository;
        _journeyRepository = journeyRepository;
        _documentsRepository = documentsRepository;
        _mdpDbUow = mdpDbUow;
        _memberDbUow = memberDbUow;
        _pensionWiseRepository = pensionWiseRepository;
        _logger = logger;
        _edmsClient = edmsClient;
        _documentFactoryProvider = documentFactoryProvider;
    }

    public async Task Start(CancellationToken stoppingToken)
    {
        await using var transaction = await _mdpDbUow.BeginTransactionAsync();
        var events = await _postIndexEventRepository.List();
        foreach (var ev in events)
            await ProcessEvent(ev);
        await transaction.CommitAsync();
    }

    private async Task ProcessEvent(RetirementPostIndexEvent ev)
    {
        try
        {
            if (ev.ReferenceNumber == "BEREAVE")
                await ProcessBereavementEvent(ev);
            else if (ev.CaseNumber == "TRANSFER")
                await ProcessTransferEvent(ev);
            else
                await ProcessRetirementEvent(ev);

            _logger.LogInformation($"Successfully processed event {ev}");
        }
        catch (Exception ex)
        {
            var message = $"Failed process event {ev}: {ex}";
            ev.SetError(message);
            _logger.LogError(message);
            await _mdpDbUow.Commit();
        }
    }

    private async Task ProcessTransferEvent(RetirementPostIndexEvent ev)
    {
        var edmsError = await _edmsClient.IndexDocument(
            ev.BusinessGroup,
            ev.ReferenceNumber,
            ev.BatchNumber);
        if (edmsError.IsLeft)
        {
            var message = $"Failed process event {ev}: {edmsError.Left().GetErrorMessage()}";
            ev.SetError(message);
            _logger.LogError(message);
            await _mdpDbUow.Commit();
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var id = await _documentsRepository.NextId();

        var document1 = _documentFactoryProvider.GetFactory(DocumentType.Transfer).Create(
            ev.BusinessGroup,
            ev.ReferenceNumber,
            id,
            ev.RetirementApplicationImageId,
            now);
        _documentsRepository.Add(document1);

        _mdpDbUow.Remove(ev);
        await _memberDbUow.Commit();
        await _mdpDbUow.Commit();
    }

    private async Task ProcessBereavementEvent(RetirementPostIndexEvent ev)
    {
        var bCase = await _caseRepository.Find(ev.CaseNumber);
        if (bCase.IsNone)
        {
            _logger.LogWarning("Warning case not found: {caseNumber}", ev.CaseNumber);
            return;
        }

        var edmsError = await _edmsDocumentsIndexing.PostIndexBereavement(
            ev.BusinessGroup,
            ev.CaseNumber,
            ev.BatchNumber);

        if (edmsError.HasValue)
        {
            var message = $"Failed process event {ev}: {edmsError.Value.Message}";
            ev.SetError(message);
            _logger.LogError(message);
            await _mdpDbUow.Commit();
            return;
        }

        _mdpDbUow.Remove(ev);
        await _mdpDbUow.Commit();
    }

    private async Task ProcessRetirementEvent(RetirementPostIndexEvent ev)
    {
        var rCase = await _caseRepository.Find(ev.CaseNumber);
        if (rCase.IsNone)
        {
            _logger.LogInformation($"Warning case not found: {ev.CaseNumber}");
            return;
        }

        var edmsError = await _edmsDocumentsIndexing.PostIndexRetirement(
            ev.ReferenceNumber,
            ev.BusinessGroup,
            ev.CaseNumber,
            "RTP9",
            ev.BatchNumber);
        if (edmsError.HasValue)
        {
            var message = $"Failed process event {ev}: {edmsError.Value.Message}";
            ev.SetError(message);
            _logger.LogError(message);
            await _mdpDbUow.Commit();
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var id = await _documentsRepository.NextId();

        var document1 = _documentFactoryProvider.GetFactory(DocumentType.Retirement).Create(
            ev.BusinessGroup,
            ev.ReferenceNumber,
            id,
            ev.RetirementApplicationImageId,
            now,
            ev.CaseNumber);
        _documentsRepository.Add(document1);

        _mdpDbUow.Remove(ev);
        await _memberDbUow.Commit();
        await _mdpDbUow.Commit();

        var currentJourney = await _journeyRepository.FindUnexpiredJourney(ev.BusinessGroup, ev.ReferenceNumber, DateTimeOffset.UtcNow);
        if (currentJourney.IsSome && currentJourney.Value().GbgId.HasValue)
        {
            var clean = await _edmsDocumentsIndexing.CleanAfterPostIndex(currentJourney.Value().GbgId.Value);
            if (clean.IsLeft || clean.Right() != HttpStatusCode.Accepted)
                _logger.LogError(clean.Left().Message ?? $"Expected status code is 202, but code: ${clean.Right()} received");
        }

        if (!currentJourney.IsSome)
        {
            _logger.LogInformation($"Successfully processed event {ev}");
            return;
        }

        var pwQuestion = currentJourney
            .Value()
            .QuestionForms(new[] { "pw_guidance" })
            .FirstOrDefault()
            .ToOption();

        if (!pwQuestion.IsSome)
        {
            _logger.LogInformation($"Successfully processed event {ev}");
            return;
        }

        var pensionWise = PensionWise.Create(ev.BusinessGroup, ev.ReferenceNumber,
            ev.CaseNumber, currentJourney.Value().FinancialAdviseDate, currentJourney.Value().PensionWiseDate,
            pwQuestion.Value().AnswerKey);

        await _pensionWiseRepository.AddAsync(pensionWise);
        await _memberDbUow.Commit();
    }
}