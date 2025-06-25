using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WTW.MdpService.BankAccounts.Services;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.CasesApi;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.EpaService;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.MdpService.Infrastructure.MemberDb.IfaReferrals;
using WTW.MdpService.Retirement;
using WTW.MdpService.SingleAuth.Services;
using WTW.Web;
using WTW.Web.Extensions;
using WTW.Web.LanguageExt;
using static WTW.Web.MdpConstants;

namespace WTW.MdpService.Content.V2;

public class AccessKeyWordingFlagsService : IAccessKeyWordingFlagsService
{
    private readonly ICalculationsParser _calculationsParser;
    private readonly ITenantRetirementTimelineRepository _tenantRetirementTimelineRepository;
    private readonly IMemberIfaReferral _memberIfaReferral;
    private readonly ITransferJourneyRepository _transferJourneyRepository;
    private readonly IJourneysRepository _journeysRepository;
    private readonly IQuoteSelectionJourneyRepository _quoteSelectionJourneyRepository;
    private readonly ICasesClient _casesClient;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IRetirementService _retirementService;
    private readonly ISingleAuthService _singleAuthService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger _logger;
    private readonly IEpaServiceClient _epaServiceClient;
    private readonly IBankService _bankService;

    public AccessKeyWordingFlagsService(ICalculationsParser calculationsParser,
        ITenantRetirementTimelineRepository tenantRetirementTimelineRepository,
        IMemberIfaReferral memberIfaReferral,
        ITransferJourneyRepository transferJourneyRepository,
        IJourneysRepository journeysRepository,
        IQuoteSelectionJourneyRepository quoteSelectionJourneyRepository,
        ICasesClient casesClient,
        ILoggerFactory loggerFactory,
        IRetirementService retirementService,
        IEpaServiceClient epaServiceClient,
        ISingleAuthService singleAuthService,
        IHttpContextAccessor httpContextAccessor,
        IBankService bankService)
    {
        _calculationsParser = calculationsParser;
        _tenantRetirementTimelineRepository = tenantRetirementTimelineRepository;
        _memberIfaReferral = memberIfaReferral;
        _transferJourneyRepository = transferJourneyRepository;
        _journeysRepository = journeysRepository;
        _quoteSelectionJourneyRepository = quoteSelectionJourneyRepository;
        _casesClient = casesClient;
        _loggerFactory = loggerFactory;
        _retirementService = retirementService;
        _singleAuthService = singleAuthService;
        _httpContextAccessor = httpContextAccessor;
        _logger = loggerFactory.CreateLogger<AccessKeyWordingFlagsService>();
        _epaServiceClient = epaServiceClient;
        _bankService = bankService;
    }

    public IEnumerable<string> GetCalcApiDatesAgesEndpointWordingFlags(Option<RetirementDatesAges> retirementDatesAges)
    {
        return retirementDatesAges.IsSome ? retirementDatesAges.Value().WordingFlags : Enumerable.Empty<string>();
    }

    public async Task<IEnumerable<string>> GetTransferWordingFlags(Option<TransferCalculation> transferCalculation)
    {
        return await transferCalculation
            .ToAsync()
            .MatchAsync<IEnumerable<string>>(
            async x =>
            {
                var flags = new List<string>();
                await _transferJourneyRepository.Find(x.BusinessGroup, x.ReferenceNumber)
                   .ToAsync()
                   .IfSome(x =>
                   {
                       flags.Add(x.TransferVersion != null ? x.TransferVersion : "transfer2");
                       flags.AddRange(x.GetQuestionFormsWordingFlags());
                   });

                var transferCalcApiFlags = Enumerable.Empty<string>();
                if (!string.IsNullOrWhiteSpace(x.TransferQuoteJson) && (transferCalcApiFlags = _calculationsParser.GetTransferQuote(x.TransferQuoteJson).WordingFlags) != null)
                    return flags.Concat(transferCalcApiFlags);

                return flags;
            },
            () => Enumerable.Empty<string>());
    }

    public async Task<IEnumerable<string>> GetIfaReferralFlags(string businessGroup, string referenceNumber)
    {
        if (await _memberIfaReferral.HasIfaReferral(referenceNumber, businessGroup, DateTimeOffset.UtcNow))
            return new List<string> { "OPENIFAREFERRAL" };

        return Enumerable.Empty<string>();
    }

    public async Task<IEnumerable<string>> GetPayTimelineWordingFlags(Member member)
    {
        var timelines = await _tenantRetirementTimelineRepository.FindPentionPayTimelines(member.BusinessGroup);
        var payTimeline = new RetirementPayTimeline(timelines, member.Category, member.SchemeCode, _loggerFactory);

        if (payTimeline.PensionPayDayIndicator() != "+")
            return new List<string> { "PD-BOTH-DAY" };

        if (int.TryParse(payTimeline.PensionPayDay(), out var day) && day > 28)
            return new List<string> { "PD-LAST-DAY" };

        return new List<string> { "PD-MONTH-DAY" };
    }

    public async Task<IEnumerable<string>> GetLinkedMemberFlags(Member member, bool isOpenAm = true)
    {
        if (!isOpenAm)
        {
            var subResult = _singleAuthService.GetSingleAuthClaim(_httpContextAccessor.HttpContext.User, MdpConstants.MemberClaimNames.Sub);

            if (!subResult.IsRight)
            {
                _logger.LogError("SubResult is not a Right value, unable to proceed with linked member retrieval.");
                return Enumerable.Empty<string>();
            }

            var linkedRecordResult = await _singleAuthService.GetLinkedRecord(subResult.Right(), member.BusinessGroup);
            if (linkedRecordResult.Count > 1)
            {
                _logger.LogInformation("Linked records found for user");
                return new List<string> { "HASLINKEDMEMBER" };
            }
            else
            {
                _logger.LogWarning("No linked records found for user");
                return new List<string>();
            }
        }

        if (member.HasLinkedMembers())
            return new List<string> { "HASLINKEDMEMBER" };

        return Enumerable.Empty<string>();
    }

    public IEnumerable<string> GetHbsFlags(Member member, Option<RetirementDatesAges> retirementDatesAges)
    {
        if (retirementDatesAges.IsNone || !member.BusinessGroup.Equals("HBS", StringComparison.InvariantCultureIgnoreCase))
            return new List<string>();

        if (retirementDatesAges.Value().NormalMinimumPensionAge == null)
            return new List<string>();

        var exactAge = member.GetExactAge(DateTimeOffset.UtcNow);
        if ((double)retirementDatesAges.Value().EarliestRetirementAge <= exactAge &&
            (double)retirementDatesAges.Value().NormalMinimumPensionAge - (double)RetirementConstants.RetirementApplicationPeriodInMonths / 12 > exactAge)
            return new List<string> { "PMP-NE" };

        if ((double)retirementDatesAges.Value().NormalMinimumPensionAge - (double)RetirementConstants.RetirementApplicationPeriodInMonths / 12 <= exactAge &&
            (double)retirementDatesAges.Value().NormalMinimumPensionAge > exactAge)
            return new List<string> { "PMP-E" };

        return new List<string>();
    }

    public IEnumerable<string> GetSchemeFlags(Member member)
    {
        return new List<string> { $"scheme_{member.SchemeCode}" };
    }

    public IEnumerable<string> GetCategoryFlags(Member member)
    {
        return new List<string> { $"category_{member.Category}" };
    }

    private async Task<bool> IsLumpSumSmallerThanMax(RetirementV2 retirementV2, Calculation calculation)
    {
        var message = $"for bgroup= {calculation.BusinessGroup}, referenceNumber= {calculation.ReferenceNumber}";
        var selectedQuoteName = calculation.SelectedQuoteName;

        if (string.IsNullOrEmpty(selectedQuoteName))
        {
            await _quoteSelectionJourneyRepository
            .Find(calculation.BusinessGroup, calculation.ReferenceNumber)
                .Match(
                        quoteSelectionJourney => quoteSelectionJourney.QuoteSelection()
                        .Match(
                                quoteName => selectedQuoteName = quoteName,
                                () => _logger.LogWarning("Selected quotename doesn't exist on given journey")
                              ),
                        () => _logger.LogWarning("Quote Journey doesn't exist")
                      );
        }

        if (string.IsNullOrEmpty(selectedQuoteName))
        {
            _logger.LogInformation($"{nameof(IsLumpSumSmallerThanMax)} - Selected option not found for {message}");
            return false;
        }

        if (selectedQuoteName.Contains("."))
        {
            selectedQuoteName = selectedQuoteName.Split(".")[0];
        }

        var selectedOptionStartString = "reducedPension";
        var totalLumpSumName = "_totalLumpSum";
        var maximumPermittedTotalLumpSumName = "_maximumPermittedTotalLumpSum";

        if (selectedQuoteName.StartsWith(selectedOptionStartString))
        {
            var selectedQuoteDetails = _retirementService.GetSelectedQuoteDetails(selectedQuoteName, retirementV2);

            if (selectedQuoteDetails.TryGetValue(selectedQuoteName + totalLumpSumName, out var totalLumpSumObject) &&
                selectedQuoteDetails.TryGetValue(selectedQuoteName + maximumPermittedTotalLumpSumName, out var maximumPermittedTotalLumpSumObject) &&
                decimal.TryParse(totalLumpSumObject.ToString(), out var totalLumpSum) &&
                decimal.TryParse(maximumPermittedTotalLumpSumObject.ToString(), out var maximumPermittedTotalLumpSum) &&
                Decimal.Compare(totalLumpSum, maximumPermittedTotalLumpSum) < 0)
            {
                _logger.LogInformation($"{nameof(IsLumpSumSmallerThanMax)} - lumpsum selected is smaller than maximum permitted {message}");
                return true;
            }
        }

        _logger.LogInformation($"{nameof(IsLumpSumSmallerThanMax)} - Check is not applicable or lumpsum selected is not smaller {message}");
        return false;
    }

    private async Task<bool> IsLumpSumSmallerThanMaxDC(RetirementV2 retirementV2, Calculation calculation)
    {
        var message = $"for bgroup= {calculation.BusinessGroup}, referenceNumber= {calculation.ReferenceNumber}";
        var selectedQuoteFullName = Option<string>.None;

        await _quoteSelectionJourneyRepository.Find(calculation.BusinessGroup, calculation.ReferenceNumber)
            .Match(
            quoteSelectionJourney => selectedQuoteFullName = quoteSelectionJourney.QuoteSelection(),
            () => _logger.LogInformation("Requested Quote Selection Journey was not found")
            );

        if (selectedQuoteFullName.IsNone)
        {
            var dcJourney = await _journeysRepository.Find(calculation.BusinessGroup, calculation.ReferenceNumber, "dcretirementapplication");
            if (dcJourney.IsNone)
            {
                _logger.LogError($"Journey not found {message}");
                return false;
            }
            var selectedQuoteDetails = dcJourney.Value().GetGenericDataByFormKey("SelectedQuoteDetails");
            if (selectedQuoteDetails.IsNull())
            {
                _logger.LogInformation("Selected Quote Details do not exist on Journey");
                return false;
            }

            selectedQuoteFullName = selectedQuoteDetails.GenericDataJson.GetValueFromJson("selectedQuoteFullName").ToOption();

            if (selectedQuoteFullName.IsNone)
            {
                _logger.LogInformation("selectedQuoteName not found");
                return false;
            }
        }

        var selectQuoteFullNameString = selectedQuoteFullName.Value();

        var selectedQuoteFromRetirementV2 = _retirementService.GetSelectedQuoteDetails(selectQuoteFullNameString, retirementV2);

        var taxFreeUFPLSName = $"{selectQuoteFullNameString}_taxFreeUFPLS";
        if (selectedQuoteFromRetirementV2.TryGetValue(taxFreeUFPLSName, out var taxFreeUFPLSObject))
        {
            Decimal.TryParse(taxFreeUFPLSObject.ToString(), out var taxFreeUFPLS);
            if (taxFreeUFPLS.IsNull())
            {
                _logger.LogInformation("taxFreeUFPLS not found on json");
                return false;
            }

            if (Decimal.Compare(taxFreeUFPLS, retirementV2.MaximumPermittedTotalLumpSum) < 0)
            {
                _logger.LogInformation($"At {nameof(IsLumpSumSmallerThanMaxDC)} - taxfreeLumpSum selected is lesser than Maximum Permitted Lump Sum {message}");
                return true;
            }
        }

        _logger.LogInformation($"At {nameof(IsLumpSumSmallerThanMaxDC)} - Check is not applicable or lumpsum selected is not smaller {message}");
        return false;
    }

    public async Task<IEnumerable<string>> GetRetirementFlags(Either<Error, Calculation> retirementCalculation, bool isSchemeDC)
    {
        var flags = Enumerable.Empty<string>();
        var v2JourneyFound = false;
        RetirementV2 retirementJsonV2 = new RetirementV2(new RetirementV2Params());

        if (retirementCalculation.IsRight && !string.IsNullOrWhiteSpace(retirementCalculation.Right().RetirementJson))
            flags = flags.Concat(_calculationsParser.GetRetirement(retirementCalculation.Right().RetirementJson).WordingFlags);
        else if (retirementCalculation.IsRight && !string.IsNullOrWhiteSpace(retirementCalculation.Right().RetirementJsonV2))
        {
            retirementJsonV2 = _calculationsParser.GetRetirementV2(retirementCalculation.Right().RetirementJsonV2);
            flags = flags.Concat(retirementJsonV2.WordingFlags);
            v2JourneyFound = true;
        }

        if (retirementCalculation.IsRight && retirementCalculation.Right().HasRetirementJourneyExpired(DateTimeOffset.UtcNow))
            flags = flags.Concat(new List<string> { "EXPIREDRA" });

        if (retirementCalculation.IsRight && retirementCalculation.Right().RetirementJourney != null)
            flags = flags.Concat(retirementCalculation.Right().RetirementJourney.GetQuestionFormsWordingFlags());


        if (retirementCalculation.IsRight && v2JourneyFound)
        {
            var addLumpsumWordingFlag = false;
            if (isSchemeDC)
            {
                addLumpsumWordingFlag = await IsLumpSumSmallerThanMaxDC(retirementJsonV2, retirementCalculation.Right());
            }
            else
            {
                addLumpsumWordingFlag = await IsLumpSumSmallerThanMax(retirementJsonV2, retirementCalculation.Right());
            }

            if (addLumpsumWordingFlag)
            {
                flags = flags.Concat(new List<string> { "RequestedLumpSumLessThanMax" });
            }
        }

        return flags;
    }

    public async Task<IEnumerable<string>> GetGenericJourneysFlags(string businessGroup, string referenceNumber)
    {
        var journeys = await _journeysRepository.FindAll(businessGroup, referenceNumber);
        return journeys.Select(x => x.Type)
            .Concat(journeys.Select(x => x.Status))
            .Concat(journeys.Select(x => x.Type + "-" + x.Status))
            .Concat(journeys.SelectMany(x => x.GetQuestionFormsWordingFlags()))
            .Concat(journeys.SelectMany(x => x.GetWordingFlags()))
            .Where(x => !string.IsNullOrWhiteSpace(x));
    }

    public async Task<IEnumerable<string>> GetQuoteSelectionFlags(string businessGroup, string referenceNumber)
    {
        var wordingFlags = new List<string>();
        var quoteSelection = await _quoteSelectionJourneyRepository.Find(businessGroup, referenceNumber);

        if (quoteSelection.IsSome)
        {
            quoteSelection.Value().QuoteSelection().IfSome(s => wordingFlags.Add(s));
            wordingFlags.AddRange(quoteSelection.Value().GetQuestionFormsWordingFlags());
        }

        if (IsMpaaQuoteSelected(wordingFlags))
            wordingFlags.Add("MPAA");

        return wordingFlags;
    }

    public async Task<IEnumerable<string>> GetRetirementOrTransferCasesFlag(string businessGroup, string referenceNumber)
    {
        var casesOption = await _casesClient.GetRetirementOrTransferCases(businessGroup, referenceNumber);
        if (casesOption.IsNone || casesOption.Value().Count() == 0)
            return Enumerable.Empty<string>();

        var flags = new List<string>();

        if (casesOption.Value().Count(x => x.CaseCode == "RTQ9" || x.CaseCode == "TOQ9") > 0)
        {
            flags.Add("QUOTE_CASES_AVAILABLE");
        }

        if (casesOption.Value().Count(x => x.CaseCode == "RTQ9" && x.CaseStatus == "Open") >= 5)
            flags.Add("overTheRetirementQuoteLimit");

        if (casesOption.Value().Count(x => x.CaseCode == "TOQ9" && x.CaseStatus == "Open") >= 1)
            flags.Add("overTheTransferQuoteLimit");

        if (casesOption.Value().Count(x => x.CaseCode == "RTP9" && x.CaseStatus == "Open" && x.CaseSource != "MDP") > 0)
        {
            flags.Add("PaperRetirementApplicationInProgress");
        }

        if (casesOption.Value().Count(x => x.CaseCode == "TOP9" && x.CaseStatus == "Open" && x.CaseSource != "MDP") > 0)
        {
            flags.Add("PaperTransferApplicationInProgress");
        }

        return flags;
    }

    private static bool IsMpaaQuoteSelected(IEnumerable<string> flags)
    {
        return flags.Any(x =>
            x.Contains("cashLumpsum") ||
            x.Contains("incomeDrawdownTFC") ||
            x.Contains("incomeDrawdownITF") ||
            x.Contains("incomeDrawdownOMTFC") ||
            x.Contains("incomeDrawdownOMITF"));
    }

    public async Task<IEnumerable<string>> GetWordingsForWebRules(Member member, string userId, List<ContentClassifierValue> webRuleWordingFlags)
    {
        _logger.LogInformation("GetWordingsForWebRules is called - member bgroup {businessGroup}, refno {referenceNumber}", member.BusinessGroup, member.ReferenceNumber);

        var applicableWebRuleWordingFlags = Enumerable.Empty<string>();

        if (webRuleWordingFlags == null)
        {
            _logger.LogInformation("webRuleWordingFlags have no value");
            return applicableWebRuleWordingFlags;
        }

        foreach (var webRuleWordingFlagValue in webRuleWordingFlags)
        {
            try
            {
                var processedWordingFlag = await ProcessWordingFlag(member, userId, webRuleWordingFlagValue);
                if (!string.IsNullOrEmpty(processedWordingFlag))
                {
                    applicableWebRuleWordingFlags = applicableWebRuleWordingFlags.Concat(new List<string> { processedWordingFlag });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetWordingsForWebRules - Failed to retrieve WebRule result for bgroup {businessGroup}, refno {referenceNumber}", member.BusinessGroup, member.ReferenceNumber);
                return applicableWebRuleWordingFlags;
            }
        }

        return applicableWebRuleWordingFlags;
    }

    private async Task<string> ProcessWordingFlag(Member member, string userId, ContentClassifierValue webRuleWordingFlagValue)
    {
        var separator = "=";
        var cachePrefix = "noCache";
        var cachePrefixSeparator = ":";

        var wordingFlag = webRuleWordingFlagValue.Key.Value;
        var webRuleValues = webRuleWordingFlagValue.Value.Value.Split(separator);
        if (webRuleValues == null || webRuleValues.Length < 2)
        {
            _logger.LogWarning("GetWordingsForWebRules - ProcessWordingFlag - Invalid ContentClassifierValue - wordingflag {wordingFlag}, for bgroup {businessGroup}, refno {referenceNumber}", wordingFlag, member.BusinessGroup, member.ReferenceNumber);
            return string.Empty;
        }

        var webRuleId = webRuleValues[0];
        var webRuleResult = webRuleValues[1];

        if (string.IsNullOrEmpty(webRuleId) || string.IsNullOrEmpty(webRuleResult))
        {
            _logger.LogWarning("GetWordingsForWebRules - ProcessWordingFlag - Invalid ContentClassifierValue - wordingflag {wordingFlag}, rule {webRuleId} , result {webRuleResult}, for bgroup {businessGroup}, refno {referenceNumber}", wordingFlag, webRuleId, webRuleResult, member.BusinessGroup, member.ReferenceNumber);
            return string.Empty;
        }

        var webRuleWithcacheOption = webRuleId.Split(cachePrefixSeparator);
        var cacheOptionFound = false;
        if (webRuleWithcacheOption.Length == 2)
        {
            if (!webRuleWithcacheOption[0].Equals(cachePrefix))
            {
                _logger.LogWarning("GetWordingsForWebRules - ProcessWordingFlag - Invalid ContentClassifierValue - wordingflag {wordingFlag}, rule {webRuleId} , result {webRuleResult}, for bgroup {businessGroup}, refno {referenceNumber}", wordingFlag, webRuleId, webRuleResult, member.BusinessGroup, member.ReferenceNumber);
                return string.Empty;
            }

            webRuleId = webRuleWithcacheOption[1];
            cacheOptionFound = true;
            _logger.LogInformation("GetWordingsForWebRules - ProcessWordingFlag - wordingflag {wordingFlag}, cachePrefix {cachePrefix} is specified - rule {webRuleId} , result {webRuleResult}, for bgroup {businessGroup}, refno {referenceNumber}", wordingFlag, cachePrefix, webRuleId, webRuleResult, member.BusinessGroup, member.ReferenceNumber);
        }

        var webRuleResultResponse = await _epaServiceClient.GetWebRuleResult(member.BusinessGroup, member.ReferenceNumber, userId, webRuleId, member.SchemeCode, cacheOptionFound);

        if (webRuleResultResponse.IsNone)
        {
            _logger.LogWarning("GetWordingsForWebRules - ProcessWordingFlag - Webrule result for bgroup {businessGroup}, refno {referenceNumber} is not retrieved for wordingflag {wordingFlag}, rule {ruleid}", member.BusinessGroup, member.ReferenceNumber, wordingFlag, webRuleId);
            return string.Empty;
        }

        _logger.LogInformation("GetWordingsForWebRules - ProcessWordingFlag - Webrule result for bgroup {businessGroup}, refno {referenceNumber} is retrieved for wordingflag {wordingFlag}, rule {ruleid} with result= {result}, expected result= {expectedResult}", member.BusinessGroup, member.ReferenceNumber, wordingFlag, webRuleId, webRuleResultResponse.Value().Result, webRuleResult);

        if (webRuleResultResponse.Value().Result.Equals(webRuleResult))
        {
            return wordingFlag;
        }

        return string.Empty;
    }

    public IEnumerable<string> GetDeathCasesWordingFlag(Member member)
    {
        if (member.IsDeathCasesLogged())
            return new List<string> { MdpConstants.WordingFlags.HASDTH };

        return Enumerable.Empty<string>();
    }
    public async Task<IEnumerable<string>> GetBankAccountWordingFlag(Member member)
    {
        var bankAccountDetails = await _bankService.FetchBankAccount(member.BusinessGroup, member.ReferenceNumber);
        return bankAccountDetails.Match(
            success =>
            {
                if (!string.IsNullOrEmpty(success.BankCountryCode) && !success.BankCountryCode.Equals(MdpConstants.UkCountryCode, StringComparison.OrdinalIgnoreCase))
                {
                    return new List<string> { MdpConstants.WordingFlags.NonUkBankCountry };
                }
                return Enumerable.Empty<string>();
            },
            error => Enumerable.Empty<string>());
    }
    public IEnumerable<string> GetNmpaFlags(DateTimeOffset? dateOfBirth)
    {
        if (!dateOfBirth.HasValue)
        {
            return Enumerable.Empty<string>();
        }

        DateTime dob = dateOfBirth.Value.Date;
        DateTime preNmpa = new DateTime(1971, 4, 6);
        DateTime nmpa = new DateTime(1973, 4, 5);

        if (dob < preNmpa) // Before 6 April 1971
        {
            return new List<string> { NMPA.Pre };
        }
        else if (dob <= nmpa) // 6 April 1971 to 5 April 1973
        {
            return new List<string> { NMPA.Current };
        }
        else // On or after 6 April 1973
        {
            return new List<string> { NMPA.Post };
        }
    }
}
