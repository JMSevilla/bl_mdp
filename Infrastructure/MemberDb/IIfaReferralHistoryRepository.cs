using System.Collections.Generic;
using System.Threading.Tasks;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb;

public interface IIfaReferralHistoryRepository
{
    Task<IEnumerable<IfaReferralHistory>> Find(string referenceNumber, string businessGroup);
}