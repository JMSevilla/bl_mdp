using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WTW.MdpService.Infrastructure.EngagementEvents;
using WTW.MdpService.Infrastructure.MemberService;
using WTW.MdpService.Infrastructure.TokenService;
using WTW.Web.Clients;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Infrastructure.MemberWebInteractionService;

public class MemberWebInteractionServiceClient : IMemberWebInteractionServiceClient
{

    private readonly HttpClient _client;
    private readonly ICachedTokenServiceClient _cachedTokenServiceClient;
    private readonly MemberWebInteractionServiceOptions _options;
    private readonly ILogger<MemberWebInteractionServiceClient> _logger;

    public MemberWebInteractionServiceClient(HttpClient client,
                               ICachedTokenServiceClient cachedTokenServiceClient,
                               IOptionsSnapshot<MemberWebInteractionServiceOptions> options,
                               ILogger<MemberWebInteractionServiceClient> logger)
    {
        _client = client;
        _cachedTokenServiceClient = cachedTokenServiceClient;
        _options = options.Value;
        _logger = logger;
    }


    public async Task<Option<MemberWebInteractionEngagementEventsResponse>> GetEngagementEvents(string businessGroup,
                                                                                                string referenceNumber)
    {
        try
        {
            return (await _client.GetJson<MemberWebInteractionEngagementEventsResponse, MemberWebInteractionResponse>(
                    string.Format(_options.GetEngagementEventsPath, businessGroup, referenceNumber),
                    ("Authorization", $"{await GetAccessToken()}")))
                .Match(
                   x => x,
                   error =>
                   {
                       _logger.LogError("Get engagements events for {businessGroup} {referenceNumber} " +
                                        "returned error: Message: {message}. Code: {code}", businessGroup, referenceNumber, error.Message, error.StatusCode);
                       return Option<MemberWebInteractionEngagementEventsResponse>.None;
                   }
                );

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve engagements events for {businessGroup} {referenceNumber}.", businessGroup, referenceNumber);
            return Option<MemberWebInteractionEngagementEventsResponse>.None;
        }

    }

    public async Task<Option<MemberMessagesResponse>> GetMessages(string businessGroup, string referenceNumber)
    {
        var result = await _client.GetOptionalJson<MemberMessagesResponse>(
                    string.Format(_options.GetMessagesPath, businessGroup, referenceNumber),
                    ("Authorization", $"{await GetAccessToken()}"));

        if (result.IsNone || result.Value().Messages == null || !result.Value().Messages.Any())
        {
            _logger.LogWarning("Messages not found for {businessGroup} {referenceNumber}.", businessGroup, referenceNumber);
            return Option<MemberMessagesResponse>.None;
        }

        var filteredMessages = result.Value().Messages
            .Where(x => !string.IsNullOrEmpty(x.MessageText))
            .ToList();

        return new MemberMessagesResponse { Messages = filteredMessages };
    }

    private async Task<string> GetAccessToken()
    {
        return "Bearer " + (await _cachedTokenServiceClient.GetAccessToken()).AccessToken;
    }
}
