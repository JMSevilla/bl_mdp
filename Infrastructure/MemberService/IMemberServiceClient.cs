using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Beneficiaries;

namespace WTW.MdpService.Infrastructure.MemberService;

public interface IMemberServiceClient
{
    Task<Option<BeneficiariesV2Response>> GetBeneficiaries(string businessGroup,
                                                           string referenceNumber,
                                                           bool includeRevoked,
                                                           bool refreshCache);
    Task<Option<GetLinkedRecordClientResponse>> GetBcUkLinkedRecord(string bgroup, string refNo);
    Task<Option<GetMemberSummaryClientResponse>> GetMemberSummary(string bgroup, string refNo);
    Task<Option<GetPensionDetailsClientResponse>> GetPensionDetails(string bgroup, string refNo);
    Task<Option<GetMemberPersonalDetailClientResponse>> GetPersonalDetail(string bgroup, string refNo);
    Task<Option<GetMatchingMemberClientResponse>> GetMemberMatchingRecords(string bgroup, GetMemberMatchingClientRequest request);
    Task<Option<MemberContactDetailsClientResponse>> GetContactDetails(string bgroup, string refNo);
}
