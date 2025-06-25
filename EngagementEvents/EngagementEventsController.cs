using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Infrastructure.EngagementEvents;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.MdpService.Infrastructure.MemberWebInteractionService;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.EngagementEvents;

[ApiController]
[Route("api/members/engagement-events")]
public class EngagementEventsController : ControllerBase
{
    private readonly ILogger<EngagementEventsController> _logger;
    private readonly IMemberRepository _memberRepository;
    private readonly IMemberWebInteractionServiceClient _memberWebInteractionServiceClient;

    public EngagementEventsController(ILogger<EngagementEventsController> logger,
                                      IMemberRepository memberRepository,
                                      IMemberWebInteractionServiceClient memberWebInteractionServiceClient)
    {
        _logger = logger;
        _memberRepository = memberRepository;
        _memberWebInteractionServiceClient = memberWebInteractionServiceClient;
    }

    [HttpGet]
    [ProducesResponseType(typeof(EngagementEventsResponse), 200)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> EngagementEvents()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();
        var member = await _memberRepository.FindMember(referenceNumber, businessGroup);
        if (member.IsNone)
        {
            _logger.LogError("Member not found for reference number {referenceNumber} and business group {businessGroup}", referenceNumber, businessGroup);
            return NotFound(ApiError.NotFound());
        }

        var internalEngagementEvents = await _memberWebInteractionServiceClient.GetEngagementEvents(businessGroup,
                                                                                                    referenceNumber);

        if (internalEngagementEvents.IsNone)
        {
            _logger.LogError("Failed to retrieve engagement events for {businessGroup} {referenceNumber}", businessGroup, referenceNumber);
            return BadRequest(ApiError.FromMessage("Failed to retrieve engagement events"));
        }

        var engagementEvents = EngagementEventsResponse.From(internalEngagementEvents.Value());

        if (engagementEvents.Events?.Any() != true)
        {
            _logger.LogInformation("No engagement events found for {businessGroup} {referenceNumber}", businessGroup, referenceNumber);
            return NoContent();
        }

        _logger.LogInformation("Engagement events returned successfully for {businessGroup} {referenceNumber}", businessGroup, referenceNumber);

        return Ok(engagementEvents);
    }
}
