using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb;

public interface IIfaReferralRepository
{
    Task<Option<IfaReferral>> Find(string referenceNumber, string businessGroup, string calcType);
}