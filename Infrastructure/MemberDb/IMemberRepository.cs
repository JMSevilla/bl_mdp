using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb;

public interface IMemberRepository
{
    Task<bool> ExistsMember(string referenceNumber, string businessGroup);
    Task<Option<Member>> FindMember(string referenceNumber, string businessGroup, string mf2FaStatus = default);
    Task<Option<List<LinkedMember>>> FindLinkedMembers(string referenceNumber, string businessGroup,
        string linkedReferenceNumber, string linkedBusinessGroup);
    Task<Option<Member>> FindMemberWithBeneficiaries(string referenceNumber, string businessGroup);
    Task<Option<Member>> FindMemberWithDependant(string referenceNumber, string businessGroup);
    Task<bool> IsMemberValidForRaCalculation(string referenceNumber, string businessGroup);
    Task<bool> IsMemberValidForTransferCalculation(string referenceNumber, string businessGroup);
    Task PopulateSessionDetails(string bGroup);
    Task DisableSysAudit(string bGroup);
}