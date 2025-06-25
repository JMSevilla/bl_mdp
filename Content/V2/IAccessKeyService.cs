using System.Collections.Generic;
using System.Threading.Tasks;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Content;

namespace WTW.MdpService.Content.V2;

public interface IAccessKeyService
{
    Task<string> CalculateKey(Member member, string userId, string tenantUrl, int preRetirementAgePeriodInYears, int newlyRetiredRangeInMonth, List<ContentClassifierValue> webRuleWordingFlags, bool useBasicMode, bool isOpenAm);
    Task<string> RecalculateKey(Member member, string userId, string tenantUrl, int preRetirementAgePeriodInYears, int newlyRetiredRangeInMonth, List<ContentClassifierValue> webRuleWordingFlags, bool useBasicMode, bool isOpenAm);
    AccessKey ParseJsonToAccessKey(string jsonString);
    string GetDcJourneyStatus(IEnumerable<string> wordingFlags);
}