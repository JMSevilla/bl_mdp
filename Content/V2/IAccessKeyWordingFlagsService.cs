using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Content;

namespace WTW.MdpService.Content.V2;

public interface IAccessKeyWordingFlagsService
{
    IEnumerable<string> GetCalcApiDatesAgesEndpointWordingFlags(Option<RetirementDatesAges> retirementDatesAges);
    IEnumerable<string> GetHbsFlags(Member member, Option<RetirementDatesAges> retirementDatesAges);
    Task<IEnumerable<string>> GetIfaReferralFlags(string businessGroup, string referenceNumber);
    Task<IEnumerable<string>> GetLinkedMemberFlags(Member member, bool isOpenAm = true);
    Task<IEnumerable<string>> GetPayTimelineWordingFlags(Member member);
    Task<IEnumerable<string>> GetRetirementFlags(Either<Error, Calculation> retirementCalculation, bool isSchemeDC);
    IEnumerable<string> GetSchemeFlags(Member member);
    Task<IEnumerable<string>> GetTransferWordingFlags(Option<TransferCalculation> transferCalculation);
    Task<IEnumerable<string>> GetGenericJourneysFlags(string businessGroup, string referenceNumber);
    Task<IEnumerable<string>> GetQuoteSelectionFlags(string businessGroup, string referenceNumber);
    IEnumerable<string> GetCategoryFlags(Member member);
    Task<IEnumerable<string>> GetRetirementOrTransferCasesFlag(string businessGroup, string referenceNumber);
    Task<IEnumerable<string>> GetWordingsForWebRules(Member member, string userId, List<ContentClassifierValue> webRuleWordingFlags);
    IEnumerable<string> GetDeathCasesWordingFlag(Member member);
    Task<IEnumerable<string>> GetBankAccountWordingFlag(Member member);
    IEnumerable<string> GetNmpaFlags(DateTimeOffset? dateOfBirth);
}