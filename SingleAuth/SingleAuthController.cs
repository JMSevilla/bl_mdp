using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WTW.MdpService.SingleAuth.Services;
using WTW.Web.Errors;

namespace WTW.MdpService.SingleAuth;

[ApiController]
[Route("api")]
public class SingleAuthController : ControllerBase
{
    private readonly ILogger<SingleAuthController> _logger;
    private readonly ISingleAuthService _singleAuthService;

    public SingleAuthController(ISingleAuthService singleAuthService, ILogger<SingleAuthController> logger)
    {
        this._singleAuthService = singleAuthService;
        _logger = logger;
    }

    [HttpPost("registration")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 500)]
    public async Task<IActionResult> Registration([Required][FromQuery] string tenantUrl)
    {
        return await _singleAuthService.RegisterUser(tenantUrl)
           .ToAsync()
           .Match<IActionResult>(
            _ =>
           {
               return NoContent();
           },
           l =>
           {
               return BadRequest(ApiError.FromMessage((l.Message)));
           });
    }

    [HttpGet("login")]
    [ProducesResponseType(typeof(SingleAuthLoginResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> Login()
    {
        return await _singleAuthService.GetLoginDetails()
           .ToAsync()
           .Match<IActionResult>(
           result =>
           {
               if (result.Count > 0)
               {
                   var response = new SingleAuthLoginResponse
                   {
                       BusinessGroup = result[0].BusinessGroup,
                       ReferenceNumber = result[0].ReferenceNumber,
                       HasMultipleRecords = result.Count > 1,
                       EligibleRecords = result.Select(x => x.ReferenceNumber).ToList(),
                   };
                   _logger.LogInformation("login successful for {bgroup} and {refno} and has multi record status as - {HasMultipleRecords}",
                    response.BusinessGroup, response.ReferenceNumber, response.HasMultipleRecords);
                   return Ok(response);
               }
               else
               {
                   _logger.LogWarning("No Record Found");
                   return NotFound(ApiError.NotFound());
               }
           },
           l =>
           {
               return BadRequest(ApiError.FromMessage((l.Message)));
           });
    }

    [HttpGet("linked-records")]
    [ProducesResponseType(typeof(LinkedRecordsResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> GetLinkedRecord()
    {
        return await _singleAuthService.GetLinkedRecordTableData()
           .ToAsync()
           .Match<IActionResult>(
           result =>
           {
               if (result.Members.Count > 0)
               {
                   _logger.LogInformation("Linked records found for user");
                   return Ok(new LinkedRecordsResponse(result));
               }
               else
               {
                   _logger.LogWarning("No linked records found for user");
                   return NotFound(ApiError.NotFound());
               }
           },
           l =>
           {
               return BadRequest(ApiError.FromMessage((l.Message)));
           });
    }

    [HttpGet("sso/outbound")]
    [ProducesResponseType(typeof(ProcessOutboundResponse), 200)]
    public async Task<IActionResult> ProcessOutbound([FromQuery] int? recordNumber, [FromQuery] bool hasMultipleRecords)
    {
        var result = await _singleAuthService.GetOutboundToken(recordNumber, hasMultipleRecords);

        return Ok(ProcessOutboundResponse.Create(result));
    }
}
