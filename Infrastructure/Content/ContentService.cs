using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace WTW.MdpService.Infrastructure.Content;

public class ContentService : IContentService
{
    private readonly IContentClient _contentClient;
    private readonly ILogger<ContentService> _logger;

    public ContentService(IContentClient contentClient, ILogger<ContentService> logger)
    {
        _contentClient = contentClient;
        _logger = logger;
    }

    public async Task<ContentResponse> FindTenant(string tenantUrl, string businessGroup)
    {
        _logger.LogInformation("FindTenant is called - tenantUrl: {tenantUrl}, businessGroup: {businessGroup}", tenantUrl, businessGroup);

        JsonElement tenant;
        try
        {
            tenant = await _contentClient.FindTenant(tenantUrl);

            return JsonConvert.DeserializeObject<ContentResponse>(tenant.GetRawText());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FindTenant - Unable to retrieve tenant content for this tenant url {tenantUrl}. Business group: {businessGroup}", tenantUrl, businessGroup);
            return null;
        }
    }

    public async Task<bool> IsValidTenant(ContentResponse tenantContent, string businessGroup)
    {
        _logger.LogInformation("IsValidTenant is called - businessGroup: {businessGroup}", businessGroup);

        if (tenantContent == null)
        {
            _logger.LogWarning("IsValidTenant - Tenant content not available, Business group: {businessGroup}", businessGroup);
            return false;
        }
        try
        {
            return tenantContent.Elements.BusinessGroup.Values.Contains(businessGroup);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IsValidTenant - Error while checking tenant content for business group: {businessGroup}", businessGroup);
            return false;
        }

    }

    public async Task<List<ContentClassifierValue>> GetWebRuleWordingFlags(ContentResponse tenantContent)
    {
        _logger.LogInformation("GetWebRuleWordingFlags is called");

        if (tenantContent == null)
        {
            _logger.LogWarning("GetWebRuleWordingFlags - Tenant content not available");
            return null;
        }

        var allWebRuleConfigurationValues = new List<ContentClassifierValue>();
        if (tenantContent?.Elements?.CmsConfiguredWordingFlags?.Values != null)
        {
            allWebRuleConfigurationValues = tenantContent.Elements.CmsConfiguredWordingFlags.Values
                .Where(cmsConfiguredWordingFlag => cmsConfiguredWordingFlag?.Elements?.WebRuleConfiguration?.Values != null)
                .SelectMany(cmsConfiguredWordingFlag => cmsConfiguredWordingFlag.Elements.WebRuleConfiguration.Values)
                .ToList();
        }
        if (allWebRuleConfigurationValues.Count > 0)
        {
            return allWebRuleConfigurationValues;
        }
        // old structure, following will be removed once new structure is in place
        else if (tenantContent?.Elements?.WebRuleWordingFlag?.Value?.Elements?.ClassifierItem?.Values != null)
        {
            return tenantContent.Elements.WebRuleWordingFlag.Value.Elements.ClassifierItem.Values;
        }
        else
        {
            _logger.LogWarning("GetWebRuleWordingFlags - Tenant content elements and/or classifieritems are not available");
            return null;
        }
    }
}