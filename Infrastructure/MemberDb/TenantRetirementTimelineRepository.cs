using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Infrastructure.MemberDb;

public class TenantRetirementTimelineRepository : ITenantRetirementTimelineRepository
{
    private readonly MemberDbContext _context;

    public TenantRetirementTimelineRepository(MemberDbContext context)
    {
        _context = context;
    }

    public async Task<ICollection<TenantRetirementTimeline>> Find(string businessGroup)
    {
        var query1 = _context.Set<TenantRetirementTimeline>().Where(x => x.BusinessGroup == businessGroup && x.OutputId == "RETFIRSTPAYMADE").OrderByDescending(x => x.SequenceNumber).Take(1);
        var query2 = _context.Set<TenantRetirementTimeline>().Where(x => x.BusinessGroup == "ZZY" && x.OutputId == "RETFIRSTPAYMADE").OrderByDescending(x => x.SequenceNumber).Take(1);
        var query3 = _context.Set<TenantRetirementTimeline>().Where(x => x.BusinessGroup == businessGroup && x.OutputId == "RETAVCSLSRECD").OrderByDescending(x => x.SequenceNumber).Take(1);
        var query4 = _context.Set<TenantRetirementTimeline>().Where(x => x.BusinessGroup == "ZZY" && x.OutputId == "RETAVCSLSRECD").OrderByDescending(x => x.SequenceNumber).Take(1);
        var query5 = _context.Set<TenantRetirementTimeline>().Where(x => x.BusinessGroup == businessGroup && x.OutputId == "RETLSRECD").OrderByDescending(x => x.SequenceNumber).Take(1);
        var query6 = _context.Set<TenantRetirementTimeline>().Where(x => x.BusinessGroup == "ZZY" && x.OutputId == "RETLSRECD").OrderByDescending(x => x.SequenceNumber).Take(1);

        return query1.Concat(query2).Concat(query3).Concat(query4).Concat(query5).Concat(query6).ToList();
    }

    public async Task<ICollection<TenantRetirementTimeline>> FindPentionPayTimelines(string businessGroup)
    {
        var query1 = _context.Set<TenantRetirementTimeline>().Where(x => x.BusinessGroup == businessGroup && x.OutputId == "RETFIRSTPAYMADE" && x.Event == "RT");
        var query2 = _context.Set<TenantRetirementTimeline>().Where(x => x.BusinessGroup == "ZZY" && x.OutputId == "RETFIRSTPAYMADE" && x.Event == "RT");
        var query3 = _context.Set<TenantRetirementTimeline>().Where(x => x.BusinessGroup == "ZZZ" && x.OutputId == "RETFIRSTPAYMADE" && x.Event == "RT");

        var result = query1.Concat(query2).Concat(query3).ToList();

        if (result.Any(x => x.BusinessGroup == businessGroup))
            return result.Where(x => x.BusinessGroup == businessGroup).ToList();

        if (result.Any(x => x.BusinessGroup == "ZZY"))
            return result.Where(x => x.BusinessGroup == "ZZY").ToList();

        return result.Where(x => x.BusinessGroup == "ZZZ").ToList();
    }
}