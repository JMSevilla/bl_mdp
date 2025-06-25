using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.Web.Authorization;
using WTW.Web.Errors;

namespace WTW.MdpService.Tenants;

[ApiController]
[Route("api/tenants/current")]
public class TenantController : ControllerBase
{
    private readonly ITenantRepository _tenantRepository;

    public TenantController(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    [HttpGet("relationship-statuses")]
    [ProducesResponseType(typeof(RelationshipStatusesResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> Relationships()
    {
        var statuses = await _tenantRepository.ListRelationships(HttpContext.User.User().BusinessGroup);
        return Ok(RelationshipStatusesResponse.From(statuses));
    }
}