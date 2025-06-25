using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Infrastructure.CasesApi;
using WTW.MdpService.Infrastructure.MemberDb.Cases;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;
using WTW.Web.Pagination;

namespace WTW.MdpService.Cases;

[ApiController]
[Route("api/cases")]
public class CasesController : ControllerBase
{
    private readonly ICasesClient _casesClient;
    private readonly ILogger<CasesController> _logger;

    public CasesController(ICasesClient casesClient, ILogger<CasesController> logger)
    {
        _casesClient = casesClient;
        _logger = logger; 
    }
    
    [HttpGet("list")]
    [ProducesResponseType(typeof(PaginatedList<CasesResponse>), 200)]
    public async Task<IActionResult> GetCaseList([FromQuery]CaseListRequest request)
    {
        (string _, string referenceNumber, string businessGroup) = HttpContext.User.User();

        var caseList = await _casesClient.GetCaseList(businessGroup, referenceNumber);
        if(caseList.IsLeft)
        {
            _logger.LogWarning("Failed to get case list for member {referenceNumber} in business group {businessGroup}. Error message: {error}:{errorMessage}", 
                referenceNumber, businessGroup, caseList.Left().Error, caseList.Left().Detail);
            return NotFound(ApiError.From("Member does not have any cases.", "member_cases_not_found"));
        }

        var caseListResponce = caseList.Right().Map(x =>
                new CaseListResponse
                {
                    CaseCode = x.CaseCode,
                    CaseNumber = x.CaseNumber,
                    CaseStatus = x.CaseStatus,
                    CreationDate = DateTime.Parse(x.CreationDate),
                    CompletionDate = !string.IsNullOrEmpty(x.CompletionDate) ? DateTime.Parse(x.CompletionDate) : null
                });
        var order = new CasesOrder().Create(request.PropertyName.ToString(), request.Ascending);

        return Ok(order
            .ApplyAsEnumerable(CasesSearchFilter.Filter(caseListResponce, request.CaseCodes)) 
            .AsPaginated(request.PageNumber != 0 && request.PageSize != 0 ? new Page(request.PageNumber, request.PageSize) : null));
    }
}