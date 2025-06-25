using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WTW.MdpService.Infrastructure.Geolocation;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.Web.Caching;
using WTW.Web.Errors;

namespace WTW.MdpService.Addresses;

[ApiController]
[Route("api/addresses")]
public class AddressesController : ControllerBase
{
    private readonly ILoqateApiClient _loqateApiClient;
    private readonly ITenantRepository _tenantRepository;

    public AddressesController(ILoqateApiClient loqateApiClient, ITenantRepository tenantRepository)
    {
        _loqateApiClient = loqateApiClient;
        _tenantRepository = tenantRepository;
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<AddressSummariesResponse>), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> FindAddressSummaries([FromQuery] AddressSummaryRequest request)
    {
        return (await _loqateApiClient.Find(request.Text, request.Container, request.Language, request.Countries))
            .Match<IActionResult>(response => Ok(response.Items.Select(AddressSummariesResponse.From)),
                error => BadRequest(ApiError.FromMessage(error.Message)));
    }

    [HttpGet("{addressId}")]
    [ProducesResponseType(typeof(IEnumerable<AddressResponse>), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> Get(string addressId)
    {
        return (await _loqateApiClient.GetDetails(addressId))
            .Match<IActionResult>(response => Ok(response.Items.Select(AddressResponse.From)),
                error => BadRequest(ApiError.FromMessage(error.Message)));
    }

    [HttpGet("countries")]
    [ProducesResponseType(typeof(IEnumerable<GetCountriesResponse>), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [CacheResponseAttribute("addresscountries", 864000)] // 10 days
    [Authorize(Policy = "BereavementInitialUserOrMember")]
    public async Task<IActionResult> GetCountries()
    {
        var response = await _tenantRepository.GetAddressCountries("WWISO", "ZZY");
        return Ok(GetCountriesResponse.From(response));
    }
}