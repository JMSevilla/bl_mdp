using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Infrastructure.CasesApi;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Documents;

[ApiController]
[Route("api/case/documents")]
public class CaseDocumentsController : ControllerBase
{
    private readonly ICaseDocumentsService _caseDocumentsService;
    private readonly ICasesClient _casesClient;
    private readonly ILogger<CaseDocumentsController> _logger;



    public CaseDocumentsController(
          ICasesClient casesClient,
          ILogger<CaseDocumentsController> logger,
          ICaseDocumentsService caseDocumentsService)
    {
        _casesClient = casesClient;
        _logger = logger;
        _caseDocumentsService = caseDocumentsService;
    }

    [HttpGet("list")]
    [ProducesResponseType(typeof(CaseDocumentsResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> ListCaseDocuments([FromQuery] CaseDocumentsRequest request)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        var caseNumberOrError = await _caseDocumentsService.GetCaseNumber(businessGroup, referenceNumber, request.CaseCode);
        if (caseNumberOrError.IsLeft)
        {
            return BadRequest(ApiError.FromMessage(caseNumberOrError.Left().Message));
        }

        var resultOrError = await _casesClient.ListDocuments(businessGroup, caseNumberOrError.Right());
        if (resultOrError.IsLeft)
        {
            _logger.LogError($"Error result form case api. Error: {resultOrError.Left().Error}. Detail: {resultOrError.Left().Detail}. Message: {resultOrError.Left().Message}.");
            return BadRequest(ApiError.FromMessage(resultOrError.Left().Detail));
        }

        return Ok(new CaseDocumentsResponse(resultOrError.Right()));
    }
}
