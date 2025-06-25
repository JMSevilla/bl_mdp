using System.Collections.Generic;
using System.Threading.Tasks;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb;

public interface ITenantRetirementTimelineRepository
{
    Task<ICollection<TenantRetirementTimeline>> Find(string businessGroup);
    Task<ICollection<TenantRetirementTimeline>> FindPentionPayTimelines(string businessGroup);
}