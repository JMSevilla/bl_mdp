using System.Collections.Generic;
using System.Threading.Tasks;

namespace WTW.MdpService.Infrastructure.Content;

public interface IContentService
{
    public Task<ContentResponse> FindTenant(string tenantUrl, string businessGroup);

    public Task<bool> IsValidTenant(ContentResponse tenantContent, string businessGroup);

    public Task<List<ContentClassifierValue>> GetWebRuleWordingFlags(ContentResponse tenantContent);
}