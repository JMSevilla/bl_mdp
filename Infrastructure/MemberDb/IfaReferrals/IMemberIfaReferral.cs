using System;
using System.Threading.Tasks;

namespace WTW.MdpService.Infrastructure.MemberDb.IfaReferrals;

public interface IMemberIfaReferral
{
    Task<bool> HasIfaReferral(string referenceNumber, string businessGroup, DateTimeOffset now);
    Task WaitForIfaReferral(string referenceNumber, string businessGroup, string calculationType, DateTimeOffset now);
}