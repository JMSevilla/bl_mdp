using System;
using System.Net.Http;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WTW.Web;
using WTW.Web.Clients;

namespace WTW.MdpService.Infrastructure.TelephoneNoteService;

public class TelephoneNoteServiceClient : ITelephoneNoteServiceClient
{
    private readonly HttpClient _client;
    private readonly TelephoneNoteServiceOptions _options;
    private readonly ILogger<TelephoneNoteServiceClient> _logger;

    public TelephoneNoteServiceClient(HttpClient client, IOptionsSnapshot<TelephoneNoteServiceOptions> options, ILogger<TelephoneNoteServiceClient> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Option<IntentContextResponse>> GetIntentContext(string businessGroup, string referenceNumber, string platform = MdpConstants.AppPlatform)
    {
        try
        {
            var uri = string.Format(_options.GetIntentContextAbsolutePath, businessGroup, referenceNumber, platform);
            return await _client.GetOptionalJson<IntentContextResponse>(uri);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting intent context for member {referenceNumber} in business group {businessGroup} with platform {platform}.", referenceNumber, businessGroup, platform);
            return Option<IntentContextResponse>.None;
        }
    }
} 