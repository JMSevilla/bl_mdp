using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Infrastructure.TelephoneNoteService;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.TelephoneNote;

[ApiController]
[Route("api/telephone-note")]
public class TelephoneNoteController : ControllerBase
{
    private readonly ITelephoneNoteServiceClient _telephoneNoteServiceClient;
    private readonly ILogger<TelephoneNoteController> _logger;

    public TelephoneNoteController(
        ITelephoneNoteServiceClient telephoneNoteServiceClient,
        ILogger<TelephoneNoteController> logger)
    {
        _telephoneNoteServiceClient = telephoneNoteServiceClient;
        _logger = logger;
    }

    [HttpGet("intent-context")]
    [ProducesResponseType(typeof(IntentContextResponse), 200)]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> GetIntentContext()
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        _logger.LogInformation("Getting intent context for member {referenceNumber} in business group {businessGroup}", referenceNumber, businessGroup);

        var result = await _telephoneNoteServiceClient.GetIntentContext(businessGroup, referenceNumber);

        return result.Match<IActionResult>(
            intentContext => Ok(intentContext),
            () => NoContent());
    }
} 