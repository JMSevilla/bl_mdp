using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Infrastructure.Edms;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.Web.Authorization;
using WTW.Web.Errors;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Documents;

[ApiController]
[Route("api/journeys/documents")]
[Authorize(Policy = "BereavementEmailVerifiedUserOrMember")]
public class JourneyDocumentsController : ControllerBase
{
    private readonly IEdmsClient _edmsClient;
    private readonly IMdpUnitOfWork _mdpDbUnitOfWork;
    private readonly IJourneyDocumentsRepository _journeyDocumentsRepository;
    private readonly ILogger<JourneyDocumentsController> _logger;

    public JourneyDocumentsController(
        IEdmsClient edmsClient,
        IMdpUnitOfWork mdpDbUnitOfWork,
        ILogger<JourneyDocumentsController> logger,
        IJourneyDocumentsRepository journeyDocumentsRepository)
    {
        _edmsClient = edmsClient;
        _mdpDbUnitOfWork = mdpDbUnitOfWork;
        _logger = logger;
        _journeyDocumentsRepository = journeyDocumentsRepository;
    }

    [HttpPost("create")]
    [ProducesResponseType(typeof(JourneyDocumentCreateResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> CreateDocument([FromForm] JourneyDocumentRequest request)
    {
        var (_, referenceNumber, businessGroup) = HttpContext.User.BereavementUserOrMember();
        var edmsResult = await _edmsClient.UploadDocument(
                       businessGroup,
                       request.File.FileName,
                       request.File.OpenReadStream());
        if (edmsResult.IsLeft)
        {
            _logger.LogWarning($"Failed to upload {request.File.FileName}. Error: {edmsResult.Left().Message}");
            return BadRequest(ApiError.FromMessage(edmsResult.Left().Message));
        }

        var document = new UploadedDocument(
            referenceNumber,
            businessGroup,
            request.JourneyType,
            request.DocumentType,
            request.File.FileName,
            edmsResult.Right().Uuid,
            DocumentSource.Incoming,
            false,
            request.Tags.ToArray());

        await _journeyDocumentsRepository.Add(document);
        await _mdpDbUnitOfWork.Commit();
        return Ok(new JourneyDocumentCreateResponse { Uuid = document.Uuid });
    }

    [HttpPut("delete")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> DeleteDocument([FromBody] JourneyDocumentDeleteRequest request)
    {
        var (_, referenceNumber, businessGroup) = HttpContext.User.BereavementUserOrMember();
        return await _journeyDocumentsRepository.Find(businessGroup, referenceNumber, request.Uuid)
            .ToAsync()
            .MatchAsync<IActionResult>(
              async document =>
              {
                  _journeyDocumentsRepository.Remove(document);
                  await _mdpDbUnitOfWork.Commit();
                  return NoContent();
              },
              () =>
              {
                  _logger.LogWarning($"File with Uuid {request.Uuid} cannot be found");
                  return NotFound(ApiError.NotFound());
              });
    }

    [HttpPut("delete/all")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> DeleteAllDocuments([FromBody] JourneyDocumentDeleteAllRequest request)
    {
        var (_, referenceNumber, businessGroup) = HttpContext.User.BereavementUserOrMember();
        var documents = await _journeyDocumentsRepository.List(businessGroup, referenceNumber, request.JourneyType);

        _journeyDocumentsRepository.RemoveAll(documents);
        await _mdpDbUnitOfWork.Commit();
        return NoContent();
    }

    [HttpPost("tags/update")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> UpdateDocumentTags([FromForm] JourneyDocumentTagUpdateRequest request)
    {
        var (_, referenceNumber, businessGroup) = HttpContext.User.BereavementUserOrMember();
        return await _journeyDocumentsRepository.Find(businessGroup, referenceNumber, request.FileUuid)
            .ToAsync()
            .MatchAsync<IActionResult>(
              async document =>
              {
                  document.UpdateTags(request.Tags);
                  await _mdpDbUnitOfWork.Commit();
                  return NoContent();
              },
              () =>
              {
                  _logger.LogWarning($"File with Uuid {request.FileUuid} cannot be found");
                  return NotFound(ApiError.NotFound());
              });
    }

    [HttpGet("list")]
    [ProducesResponseType(typeof(IEnumerable<JourneyDocumentsResponse>), 200)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> ListDocuments([FromQuery] JourneyDocumentListRequest request)
    {
        var (_, referenceNumber, businessGroup) = HttpContext.User.BereavementUserOrMember();
        var documents = await _journeyDocumentsRepository.List(businessGroup, referenceNumber, request.JourneyType);
        return Ok(documents.Select(x => new JourneyDocumentsResponse(x)));
    }
}