using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.MdpService.Infrastructure.MemberDb.Documents;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.TransferJourneys;

public class TransferOutsideAssure : ITransferOutsideAssure
{
    private readonly ICalculationsClient _calculationsClient;
    private readonly ITransferJourneyRepository _transferJourneyRepository;
    private readonly ITransferCalculationRepository _transferCalculationRepository;
    private readonly IDocumentsRepository _documentsRepository;
    private readonly IDocumentFactoryProvider _documentFactoryProvider;
    private readonly IMemberDbUnitOfWork _memberDbUnitOfWork;
    private readonly IMdpUnitOfWork _mdpUnitOfWork;
    private readonly ICalculationsParser _calculationsParser;
    private readonly ILogger<TransferOutsideAssure> _logger;

    public TransferOutsideAssure(ICalculationsClient calculationsClient,
        ITransferJourneyRepository transferJourneyRepository,
        ITransferCalculationRepository transferCalculationRepository,
        IDocumentsRepository documentsRepository,
        IDocumentFactoryProvider documentFactoryProvider,
        IMemberDbUnitOfWork memberDbUnitOfWork,
        IMdpUnitOfWork mdpUnitOfWork,
        ICalculationsParser calculationsParser,
        ILogger<TransferOutsideAssure> logger)
    {
        _calculationsClient = calculationsClient;
        _transferJourneyRepository = transferJourneyRepository;
        _transferCalculationRepository = transferCalculationRepository;
        _documentsRepository = documentsRepository;
        _documentFactoryProvider = documentFactoryProvider;
        _memberDbUnitOfWork = memberDbUnitOfWork;
        _mdpUnitOfWork = mdpUnitOfWork;
        _calculationsParser = calculationsParser;
        _logger = logger;
    }

    public async Task CreateTransferForEpa(Member member)
    {
        if (member.TransferPaperCase().IsSome)
        {
            var now = DateTimeOffset.UtcNow;

            var calculation = new TransferCalculation(member.BusinessGroup, member.ReferenceNumber, null, now);
            calculation.LockTransferQoute();
            calculation.SetStatus(TransferApplicationStatus.OutsideTA);
            await _transferCalculationRepository.Create(calculation);

            var journey = TransferJourney.CreateEpa(member.BusinessGroup,
                member.ReferenceNumber,
                member.TransferPaperCase().Value().CaseNumber,
                member.TransferPaperCase().Value().CaseReceivedDate.Value.ToUniversalTime(), now);

            await _transferJourneyRepository.Create(journey);
        }
    }

    public async Task CreateTransferForLockedQuote(string referenceNumber, string businessGroup)
    {
        var datesAgesResponse = await _calculationsClient.RetirementDatesAges(referenceNumber, businessGroup).Try();

        if (datesAgesResponse.IsSuccess &&
            datesAgesResponse.Value().HasLockedInTransferQuote &&
            (await _transferJourneyRepository.Find(businessGroup, referenceNumber)).IsNone)
        {
            var transferResponseOrError = await _calculationsClient.TransferCalculation(businessGroup, referenceNumber);
            if (transferResponseOrError.IsLeft)
            {
                _logger.LogError($"Failed to retrieve Transfer data from calc api. Business group:{businessGroup}," +
                    $" Refno:{referenceNumber}. Error: \'{transferResponseOrError.Left().Message}\'.");
                return;
            }

            if (datesAgesResponse.Value().LockedInTransferQuoteImageId == null)
                _logger.LogError($"Failed to retrieve transfer quote image id. Business group:{businessGroup}," +
                    $" Refno:{referenceNumber}.");

            await CreateCalculation(referenceNumber, businessGroup, transferResponseOrError);
            await CreateJourney(referenceNumber, businessGroup, datesAgesResponse);
            await CreateDocument(referenceNumber, businessGroup, datesAgesResponse);

            await _mdpUnitOfWork.Commit();
            await _memberDbUnitOfWork.Commit();
        }

        datesAgesResponse.IfFail(error =>
        {
            _logger.LogError(error, $"Failed to retrieve HasLockedInTransferQuote value from calc api. Business group:{businessGroup}, Refno:{referenceNumber}.");
        });
    }

    private async Task CreateDocument(string referenceNumber, string businessGroup, LanguageExt.Common.Result<RetirementDatesAgesResponse> datesAgesResponse)
    {
        var memberDocument = _documentFactoryProvider.GetFactory(DocumentType.TransferV2OutsideAssureQuoteLock).Create(
                               businessGroup,
                               referenceNumber,
                               await _documentsRepository.NextId(),
                               datesAgesResponse.Value().LockedInTransferQuoteImageId ?? 0,
                               DateTimeOffset.UtcNow);
        _documentsRepository.Add(memberDocument);
    }

    private async Task CreateJourney(string referenceNumber, string businessGroup, LanguageExt.Common.Result<RetirementDatesAgesResponse> datesAgesResponse)
    {
        var journey = TransferJourney.Create(
                    businessGroup,
                    referenceNumber,
                    DateTimeOffset.UtcNow,
                    "hub",
                    "t2_guaranteed_value_2",
                    datesAgesResponse.Value().LockedInTransferQuoteImageId ?? 0);
        await _transferJourneyRepository.Create(journey);
    }

    private async Task CreateCalculation(string referenceNumber, string businessGroup, LanguageExt.Either<LanguageExt.Common.Error, TransferResponse> transferResponseOrError)
    {
        var transferQuoteJson = _calculationsParser.GetTransferQuoteJson(transferResponseOrError.Right());
        var transferQuote = _calculationsParser.GetTransferQuote(transferQuoteJson);
        var newTransferCalculation = new TransferCalculation(businessGroup, referenceNumber, transferQuoteJson, DateTimeOffset.UtcNow);
        newTransferCalculation.LockTransferQoute();
        await _transferCalculationRepository.Create(newTransferCalculation);
    }
}