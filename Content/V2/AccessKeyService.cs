using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.Investment;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.Web;
using WTW.Web.Caching;
using WTW.Web.LanguageExt;
using WTW.Web.Serialization;
using static WTW.MdpService.Retirement.GuaranteedQuotesRequest;

namespace WTW.MdpService.Content.V2;

public class AccessKeyService : IAccessKeyService
{
    private readonly Error error = Error.New("Calculation Api is not accessible for member", "calcNotAccessible");
    private readonly ICalculationsClient _calculationsClient;
    private readonly IRetirementAccessKeyDataService _retirementAccessKeyDataService;
    private readonly ITransferCalculationRepository _transferCalculationRepository;
    private readonly IAccessKeyWordingFlagsService _accessKeyWordingFlagsService;
    private readonly ICalculationsParser _calculationsParser;
    private readonly ICache _cache;
    private readonly ILogger<AccessKeyService> _logger;
    private readonly IWebChatFlagService _webChatFlagService;
    private readonly IInvestmentServiceClient _investmentServiceClient;

    public AccessKeyService(ICalculationsClient calculationsClient,
        IRetirementAccessKeyDataService retirementAccessKeyDataService,
        ITransferCalculationRepository transferCalculationRepository,
        IAccessKeyWordingFlagsService accessKeyWordingFlagsService,
        ICalculationsParser calculationsParser,
        ICache cache,
        ILogger<AccessKeyService> logger,
        IWebChatFlagService webChatFlagService,
        IInvestmentServiceClient investmentServiceClient)
    {
        _calculationsClient = calculationsClient;
        _retirementAccessKeyDataService = retirementAccessKeyDataService;
        _transferCalculationRepository = transferCalculationRepository;
        _accessKeyWordingFlagsService = accessKeyWordingFlagsService;
        _calculationsParser = calculationsParser;
        _cache = cache;
        _logger = logger;
        _webChatFlagService = webChatFlagService;
        _investmentServiceClient = investmentServiceClient;
    }

    public async Task<string> CalculateKey(Member member, string userId, string tenantUrl, int preRetirementAgePeriodInYears, int newlyRetiredRangeInMonth, List<ContentClassifierValue> webRuleWordingFlags, bool useBasicMode, bool isOpenAm)
    {
        _logger.LogInformation("CalculateKey is called - member bgroup {businessGroup}, {referenceNumber}, {userid}", member.ReferenceNumber, member.BusinessGroup, userId);

        if (useBasicMode)
        {
            _logger.LogInformation($"Bulding Basic access key using {nameof(BuildAccessKeyBasic)}");
            return await BuildAccessKeyBasic(member, tenantUrl, isOpenAm, userId, webRuleWordingFlags);
        }

        Either<Error, Calculation> retirementCalculation = error;
        Either<Error, RetirementDatesAges> retirementDatesAges = error;

        var retirementDatesAgesResponse = await _calculationsClient.RetirementDatesAges(member.ReferenceNumber, member.BusinessGroup).Try();
        retirementDatesAgesResponse.IfFail(x => _logger.LogWarning(x, "Calc api RetirementDatesAges endpoint failed to return data."));

        if (retirementDatesAgesResponse.IsSuccess)
        {
            retirementDatesAges = new RetirementDatesAges(retirementDatesAgesResponse.Value());

            retirementCalculation = await _retirementAccessKeyDataService.GetExistingRetirementJourneyType(member) switch
            {
                ExistingRetirementJourneyType.DbRetirementApplication => await _retirementAccessKeyDataService.GetRetirementCalculationWithJourney(retirementDatesAgesResponse.Value(), member.ReferenceNumber, member.BusinessGroup),
                ExistingRetirementJourneyType.DcRetirementApplication => (await _retirementAccessKeyDataService.GetRetirementCalculation(member.ReferenceNumber, member.BusinessGroup)).Value(),
                ExistingRetirementJourneyType.None => await _retirementAccessKeyDataService.GetNewRetirementCalculation(retirementDatesAgesResponse.Value(), member)
            };
        }

        return await BuildAccessKey(member, userId, tenantUrl, preRetirementAgePeriodInYears, newlyRetiredRangeInMonth, retirementCalculation, retirementDatesAges, webRuleWordingFlags);
    }

    public async Task<string> RecalculateKey(Member member, string userId, string tenantUrl, int preRetirementAgePeriodInYears, int newlyRetiredRangeInMonth, List<ContentClassifierValue> webRuleWordingFlags, bool useBasicMode, bool isOpenAm)
    {
        _logger.LogInformation("RecalculateKey is called - member bgroup {businessGroup}, {referenceNumber}, {userid}", member.ReferenceNumber, member.BusinessGroup, userId);

        if (useBasicMode)
        {
            _logger.LogInformation($"Bulding Basic access key using {nameof(BuildAccessKeyBasic)}");
            return await BuildAccessKeyBasic(member, tenantUrl, isOpenAm, userId, webRuleWordingFlags);
        }

        Either<Error, Calculation> retirementCalculation = error;
        Either<Error, RetirementDatesAges> retirementDatesAges = error;

        var persistedCalculation = await _retirementAccessKeyDataService.GetRetirementCalculation(member.ReferenceNumber, member.BusinessGroup);
        if (persistedCalculation.IsNone)
        {
            _logger.LogInformation("No persisted calculation for member: {bgroup}, {referenceNumber}", member.BusinessGroup, member.ReferenceNumber);

            var retirementDatesAgesResponse = await _calculationsClient.RetirementDatesAges(member.ReferenceNumber, member.BusinessGroup).Try();
            retirementDatesAgesResponse.IfFail(x => _logger.LogWarning(x, "Calc api RetirementDatesAges endpoint failed to return data."));
            if (retirementDatesAgesResponse.IsSuccess)
            {
                retirementDatesAges = new RetirementDatesAges(retirementDatesAgesResponse.Value());
                retirementCalculation = await _retirementAccessKeyDataService.GetNewRetirementCalculation(retirementDatesAgesResponse.Value(), member);
            }

        }
        else if (!IsRetirementFlaggedAsFailed(persistedCalculation))
        {
            _logger.LogInformation("Persisted calculation is not flagged as failed");

            if (member.IsSchemeDc())
            {
                await _cache.Remove($"calc-api-{member.ReferenceNumber}-{member.BusinessGroup}-retirement-dates-ages");
                var retirementDatesAgesResponse = await _calculationsClient.RetirementDatesAges(member.ReferenceNumber, member.BusinessGroup).Try();
                retirementDatesAgesResponse.IfFail(x => _logger.LogWarning(x, "Calc api RetirementDatesAges endpoint failed to return data."));
                if (retirementDatesAgesResponse.IsSuccess)
                {
                    await _retirementAccessKeyDataService.UpdateRetirementDatesAges(persistedCalculation.Value(), retirementDatesAgesResponse.Value());
                    retirementDatesAges = _calculationsParser.GetRetirementDatesAges(persistedCalculation.Value().RetirementDatesAgesJson);
                }
            }
            else
                retirementDatesAges = _calculationsParser.GetRetirementDatesAges(persistedCalculation.Value().RetirementDatesAgesJson);

            retirementCalculation = persistedCalculation.Value();
        }

        return await BuildAccessKey(member, userId, tenantUrl, preRetirementAgePeriodInYears, newlyRetiredRangeInMonth, retirementCalculation, retirementDatesAges, webRuleWordingFlags);
    }

    public AccessKey ParseJsonToAccessKey(string jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
            return null;

        return JsonSerializer.Deserialize<AccessKey>(jsonString.Replace("\\", ""));
    }

    public string GetDcJourneyStatus(IEnumerable<string> wordingFlags)
    {
        if (wordingFlags == null)
            return null;

        if (wordingFlags.Contains(MdpConstants.DcJourneyStatus.DcRetirementApplicationSubmitted))
            return MdpConstants.DcJourneyStatus.Submitted;
        if (wordingFlags.Contains(MdpConstants.DcJourneyStatus.DcRetirementApplicationStarted))
            return MdpConstants.DcJourneyStatus.Started;
        if (wordingFlags.Contains(MdpConstants.DcJourneyStatus.DcExploreOptionsStarted))
            return MdpConstants.DcJourneyStatus.ExploreOptions;

        return null;
    }

    private bool IsRetirementFlaggedAsFailed(Option<Calculation> persistedCalculation)
    {
        return !string.IsNullOrWhiteSpace(persistedCalculation.Value().RetirementJsonV2) &&
            (_calculationsParser.GetRetirementV2(persistedCalculation.Value().RetirementJsonV2)).IsCalculationFailed();
    }

    private string GetDbCalculationStatus(Either<Error, Calculation> retirementCalculation)
    {
        if (retirementCalculation.IsRight)
            return retirementCalculation.Right().CalculationStatus;

        return retirementCalculation.Left().Inner.IsSome ? retirementCalculation.Left().Inner.Value().Message : "calcNotAccessible";
    }

    private string GetDcLifeStage(Either<Error, Calculation> retirementCalculation, bool isForbidden)
    {
        if (retirementCalculation.IsLeft || isForbidden)
            return "undefined";

        var retirementDatesAges = _calculationsParser.GetRetirementDatesAges(retirementCalculation.Right().RetirementDatesAgesJson);
        return retirementDatesAges.GetDCLifeStageStatus(DateTimeOffset.UtcNow);
    }

    private async Task<bool> GetWebchatFlag(Member member)
    {
        var bGroup = member.BusinessGroup;
        var schemeCode = member.SchemeCode;
        var memberStatus = member.StatusCode;

        if (!await _webChatFlagService.IsWebChatEnabledForBusinessGroup(bGroup))
            return false;

        return await _webChatFlagService.IsWebChatEnabledForMemberCriteria(bGroup, schemeCode, memberStatus);
    }

    private static MemberLifeStage GetLifeStage(Member member,
        Option<RetirementDatesAges> retirementDatesAges,
        int preRetirementAgePeriodInYears,
        int newlyRetiredRangeInMonth)
    {
        if (retirementDatesAges.IsNone)
            return MemberLifeStage.Undefined;

        return member.GetLifeStage(
            DateTimeOffset.UtcNow,
            preRetirementAgePeriodInYears,
            newlyRetiredRangeInMonth,
            retirementDatesAges.Value());
    }

    private bool HasAdditionalContributions(Either<Error, Calculation> retirementCalculation)
    {
        return retirementCalculation.Match(
            x => !string.IsNullOrWhiteSpace(x.RetirementJsonV2) && _calculationsParser.GetRetirementV2(x.RetirementJsonV2).HasAdditionalContributions(),
            _ => false);
    }

    private async Task<string> BuildAccessKey(
        Member member,
        string userId,
        string tenantUrl,
        int preRetirementAgePeriodInYears,
        int newlyRetiredRangeInMonth,
        Either<Error, Calculation> retirementCalculation,
        Either<Error, RetirementDatesAges> retirementDatesAgesResponse,
        List<ContentClassifierValue> webRuleWordingFlags)
    {
        _logger.LogInformation("BuildAccessKey is called - member bgroup {businessGroup}, {referenceNumber}, {userid}", member.ReferenceNumber, member.BusinessGroup, userId);

        Option<RetirementDatesAges> retirementDatesAges = null;
        if (retirementDatesAgesResponse.IsLeft)
        {
            retirementDatesAges = Option<RetirementDatesAges>.None;
        }
        else
        {
            retirementDatesAges = retirementCalculation.IsLeft
                ? retirementDatesAgesResponse.Right()
                : _calculationsParser.GetRetirementDatesAges(retirementCalculation.Right().RetirementDatesAgesJson);
        }

        var isCalculationSuccessful = retirementCalculation.IsRight ? retirementCalculation.Right().IsCalculationSuccessful : false;
        var hasAdditionalContributions = HasAdditionalContributions(retirementCalculation);
        var schemeType = member.Scheme.Type;
        var memberStatus = member.Status;
        var dbCalculationStatus = GetDbCalculationStatus(retirementCalculation);
        var lifeStage = GetLifeStage(member, retirementDatesAges, preRetirementAgePeriodInYears, newlyRetiredRangeInMonth);
        var retirementApplicationStatus = _retirementAccessKeyDataService.GetRetirementApplicationStatus(member, retirementCalculation, preRetirementAgePeriodInYears, newlyRetiredRangeInMonth);

        var transferCalculation = await _transferCalculationRepository.Find(member.BusinessGroup, member.ReferenceNumber);
        var transferApplicationStatus = member.GetTransferApplicationStatus(transferCalculation);

        var wordingFlags = await CollectWordingFlags(member, userId, retirementDatesAges, retirementCalculation, transferCalculation, webRuleWordingFlags);

        // should be calculated only if calc is dc?
        var dcLifeStage = GetDcLifeStage(retirementCalculation, dbCalculationStatus == "forbidden");
        bool isWebChatEnabled = await GetWebchatFlag(member);

        bool hasProtectedQuote = false;
        int numberOfProtectedQuotes = 0;

        if (_calculationsParser.IsGuaranteedQuoteEnabled(member.BusinessGroup))
        {
            var getGuaranteedQuoteClientRequest = new GetGuaranteedQuoteClientRequest
            {
                Bgroup = member.BusinessGroup,
                RefNo = member.ReferenceNumber,
                QuotationStatus = QuotationStatusValue.GUARANTEED.ToString(),
            };

            var guaranteedQuotes = await _calculationsClient.GetGuaranteedQuotes(getGuaranteedQuoteClientRequest);

            hasProtectedQuote = (guaranteedQuotes.IsRight && guaranteedQuotes.Right().Quotations.Any());

            if (guaranteedQuotes.IsRight)
            {
                var protectedQuotes = guaranteedQuotes.Right().Quotations;
                numberOfProtectedQuotes = protectedQuotes.Count;
                if (protectedQuotes.Any(x => x.EffectiveDate.HasValue &&
                             x.EffectiveDate.Value.ToString(MdpConstants.DateFormat) ==
                             retirementCalculation.Right()?.EffectiveRetirementDate.ToString(MdpConstants.DateFormat)))
                {
                    wordingFlags = wordingFlags.Append(MdpConstants.SelectedDateProtected);
                }

            }
        }

        return JsonSerializer.Serialize(new
        {
            tenantUrl,
            isCalculationSuccessful,
            hasAdditionalContributions,
            schemeType,
            memberStatus,
            lifeStage,
            retirementApplicationStatus,
            transferApplicationStatus,
            wordingFlags,
            currentAge = member.GetAgeAndMonth(DateTimeOffset.UtcNow),
            dbCalculationStatus,
            dcLifeStage,
            isWebChatEnabled,
            hasProtectedQuote,
            numberOfProtectedQuotes
        }, SerialiationBuilder.Options());
    }

    private async Task<string> BuildAccessKeyBasic(Member member, string tenantUrl, bool isOpenAm, string userId, List<ContentClassifierValue> webRuleWordingFlags)
    {
        var wordingFlags = await CollectWordingFlagsBasic(member, isOpenAm, userId, webRuleWordingFlags);

        return JsonSerializer.Serialize(new
        {
            tenantUrl = tenantUrl,
            wordingFlags = wordingFlags
        }, SerialiationBuilder.Options());
    }

    async Task<IEnumerable<string>> GetAvcsFlags(Member member)
    {
        var flags = Enumerable.Empty<string>();

        if (member.Scheme.Type == "DC")
            return flags;

        //Internal balance check
        var internalBalance = await _investmentServiceClient.GetInternalBalance(member.ReferenceNumber, member.BusinessGroup, member.Scheme.Type);
        if (internalBalance.IsSome)
        {
            _logger.LogInformation("Internal balance for {businessGroup} {referenceNumber} has been retrieved.", member.BusinessGroup, member.ReferenceNumber);
            if (internalBalance.Value().TotalValue > 0)
                return new[] { "HasDCAssets" };
        }
        _logger.LogWarning("Internal balance for {businessGroup} {referenceNumber} retrieval failure", member.BusinessGroup, member.ReferenceNumber);

        return flags;
    }

    private async Task<IEnumerable<string>> CollectWordingFlags(
        Member member,
        string userId,
        Option<RetirementDatesAges> retirementDatesAges,
        Either<Error, Calculation> retirementCalculation,
        Option<TransferCalculation> transferCalculation,
        List<ContentClassifierValue> webRuleWordingFlags)
    {
        _logger.LogInformation("CollectWordingFlags is called - member bgroup {businessGroup}, {referenceNumber}, {userid}", member.ReferenceNumber, member.BusinessGroup, userId);

        return (await _accessKeyWordingFlagsService.GetRetirementFlags(retirementCalculation, member.IsSchemeDc()))
            .Concat(_accessKeyWordingFlagsService.GetSchemeFlags(member))
            .Concat(_accessKeyWordingFlagsService.GetCategoryFlags(member))
            .Concat(await _accessKeyWordingFlagsService.GetIfaReferralFlags(member.BusinessGroup, member.ReferenceNumber))
            .Concat(_accessKeyWordingFlagsService.GetHbsFlags(member, retirementDatesAges))
            .Concat(await _accessKeyWordingFlagsService.GetLinkedMemberFlags(member))
            .Concat(await _accessKeyWordingFlagsService.GetPayTimelineWordingFlags(member))
            .Concat(_accessKeyWordingFlagsService.GetCalcApiDatesAgesEndpointWordingFlags(retirementDatesAges))
            .Concat(await _accessKeyWordingFlagsService.GetTransferWordingFlags(transferCalculation))
            .Concat(await _accessKeyWordingFlagsService.GetGenericJourneysFlags(member.BusinessGroup, member.ReferenceNumber))
            .Concat(await _accessKeyWordingFlagsService.GetQuoteSelectionFlags(member.BusinessGroup, member.ReferenceNumber))
            .Concat(await _accessKeyWordingFlagsService.GetRetirementOrTransferCasesFlag(member.BusinessGroup, member.ReferenceNumber))
            .Concat(await GetAvcsFlags(member))
            .Concat(await _accessKeyWordingFlagsService.GetWordingsForWebRules(member, userId, webRuleWordingFlags))
            .Concat(_accessKeyWordingFlagsService.GetDeathCasesWordingFlag(member))
            .Concat(await _accessKeyWordingFlagsService.GetBankAccountWordingFlag(member))
            .Concat(_accessKeyWordingFlagsService.GetNmpaFlags(member.PersonalDetails.DateOfBirth));
    }

    private async Task<IEnumerable<string>> CollectWordingFlagsBasic(Member member, bool isOpenAm, string userId, List<ContentClassifierValue> webRuleWordingFlags)
    {
        _logger.LogInformation("{CollectWordingFlagsBasic} method was called", nameof(CollectWordingFlagsBasic));
        return (await _accessKeyWordingFlagsService.GetLinkedMemberFlags(member, isOpenAm))
            .Concat(await _accessKeyWordingFlagsService.GetWordingsForWebRules(member, userId, webRuleWordingFlags))
            .Concat(_accessKeyWordingFlagsService.GetSchemeFlags(member))
            .Concat(_accessKeyWordingFlagsService.GetCategoryFlags(member));
    }
}