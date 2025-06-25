using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Infrastructure.Compressions;
using WTW.MdpService.Infrastructure.Edms;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.MdpService.Infrastructure.MemberDb.Documents;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.Pagination;

namespace WTW.MdpService.Documents;

[ApiController]
[Route("api/members/current/documents")]
public class PersonalDocumentsController : ControllerBase
{
    private readonly IDocumentsRepository _repository;
    private readonly IEdmsClient _edmsClient;
    private readonly IMemberDbUnitOfWork _uow;
    private readonly ILogger<PersonalDocumentsController> _logger;

    public PersonalDocumentsController(
        IDocumentsRepository repository,
        IEdmsClient edmsClient,
        IMemberDbUnitOfWork uow,
        ILogger<PersonalDocumentsController> logger)
    {
        _repository = repository;
        _edmsClient = edmsClient;
        _uow = uow;
        _logger = logger;
    }

    [HttpGet("types")]
    [ProducesResponseType(typeof(DocumentsTypesResponse), 200)]
    public async Task<IActionResult> RetrieveDocumentsTypes()
    {
        return Ok(new DocumentsTypesResponse(await _repository.FindTypes(
            HttpContext.User.User().ReferenceNumber,
            HttpContext.User.User().BusinessGroup)));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<DocumentResponse>), 200)]
    public async Task<IActionResult> RetrieveDocuments([FromQuery] DocumentsRequest request)
    {
        var documents = await _repository.List(
            new DocumentsSearchSpec(
                HttpContext.User.User().ReferenceNumber,
                HttpContext.User.User().BusinessGroup,
                request.Name,
                request.Type,
                request.DocumentReadStatus?.ToString(),
                request.ReceivedDateFrom,
                request.ReceivedDateTo),
            DocumentsOrder.Create(request.PropertyName.ToString(), request.Ascending),
            new Page(request.PageNumber, request.PageSize));

        return Ok(documents.Select(DocumentResponse.From));
    }

    [HttpGet("{id}/download")]
    [ProducesResponseType(typeof(FileResult), 200, "application/octet-stream")]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> DownloadDocument(int id)
    {
        return await _repository.FindByDocumentId(
            HttpContext.User.User().ReferenceNumber,
            HttpContext.User.User().BusinessGroup,
            id)
            .ToAsync()
            .MatchAsync<IActionResult>(
                async document =>
                {
                    var file = await _edmsClient.GetDocument(document.ImageId);
                    document.MarkAsRead(DateTimeOffset.UtcNow);
                    await _uow.Commit();
                    return File(file, "application/octet-stream", document.FileName);
                },
                () => NotFound(ApiError.NotFound()));
    }

    [HttpGet("{id}/download/protected-quote")]
    [ProducesResponseType(typeof(FileResult), 200, "application/octet-stream")]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> DownloadProtectedQuoteDocument(int id)
    {
        (_, string referenceNumber, string businessGroup) = HttpContext.User.User();

        var fileStream = await _edmsClient.GetDocumentOrError(id);
        return fileStream.Match<IActionResult>(
            file =>
            {
                return File(file, "application/octet-stream", $"{businessGroup}-{referenceNumber}-{id}.pdf");
            },
            error =>
            {
                _logger.LogWarning($"Failed to download document with ID {id}. Error: {error.Message}");
                return NotFound();
            });
    }

    [HttpGet("download")]
    [ProducesResponseType(typeof(FileResult), 200, "application/octet-stream")]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> DownloadDocuments([FromQuery][Required] int[] ids)
    {
        var documents = await _repository.List(
           HttpContext.User.User().ReferenceNumber,
           HttpContext.User.User().BusinessGroup,
           ids);

        if (ids.Length == 0 || documents.Count != ids.Length)
            return NotFound(ApiError.NotFound());

        var contents = await _edmsClient.GetDocuments(documents);
        var zip = await FileCompression.Zip(DocumentsPool.Aggregate(contents, documents));

        return File(zip, "application/zip", $"{Guid.NewGuid()}.zip");
    }
}