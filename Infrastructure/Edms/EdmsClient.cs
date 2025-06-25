using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using Polly;
using WTW.MdpService.Domain.Common;
using WTW.Web.Clients;

namespace WTW.MdpService.Infrastructure.Edms;

public class EdmsClient : IEdmsClient
{
    private readonly HttpClient _client;
    private readonly string _userName;
    private readonly string _password;
    private readonly ILogger<EdmsClient> _logger;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy =
        Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .OrResult(x => x.StatusCode is HttpStatusCode.NotFound)
        .WaitAndRetryAsync(20, retryCount => TimeSpan.FromSeconds(2));

    public EdmsClient(HttpClient client, string userName, string password, ILogger<EdmsClient> logger)
    {
        _client = client;
        _userName = userName;
        _password = password;
        _logger = logger;
    }

    public async Task<Either<Error, Stream>> GetDocumentOrError(int id)
    {
        try
        {
            return await GetDocument(id);
        }
        catch (Exception e)
        {
            return Error.New(e.Message);
        }
    }

    public async Task<Stream> GetDocument(int id)
    {
        return await GetDocument(id, await GetAuthorizationHeader());
    }

    public async Task<ICollection<(int Id, Stream Stream)>> GetDocuments(ICollection<Domain.Members.Document> documents)
    {
        var header = await GetAuthorizationHeader();
        return await Task.WhenAll(documents.Select(async document => (document.Id, await GetDocument(document.ImageId, header))));
    }

    public async Task<Either<DocumentUploadError, DocumentUploadResponse>> UploadDocument(
       string businessGroup,
       string fileName,
       Stream blob,
       int? batchNumber = null)
    {
        await using var memoryStream = new MemoryStream();
        await blob.CopyToAsync(memoryStream);

        return await UploadDocumentBase64(businessGroup, fileName, Convert.ToBase64String(memoryStream.ToArray()), batchNumber);
    }

    public async Task<Either<PreindexError, PreindexResponse>> PreindexDocument(
        string businessGroup,
        string referenceNumber,
        string client,
        MemoryStream blob,
        int? batchNumber = null)
    {
        return await _client.PostJson<PreindexRequest, PreindexResponse, PreindexError>(
            "v1/preindex",
            new PreindexRequest
            {
                BusinessGroup = businessGroup,
                Client = client,
                ReferenceNumber = referenceNumber,
                FileBlob = Convert.ToBase64String(blob.ToArray()),
                BatchNumber = batchNumber
            },
            await GetAuthorizationHeader());
    }

    public async Task<Either<PostIndexError, PostindexDocumentsResponse>> PostindexDocuments(
        string businessGroup,
        string referenceNumber,
        string caseNumber,
        IList<UploadedDocument> documents)
    {
        var request = new PostindexDocumentsRequest
        {
            BusinessGroup = businessGroup,
            Client = $"{businessGroup}1",
            ReferenceNumber = referenceNumber,
            CaseNumber = caseNumber,
            Documents = documents.Select(doc => new PostindexDocumentRequest(doc.Uuid, doc.Tags?.Split(';').ToList() ?? new List<string>(), doc.IsEdoc, doc.IsEpaOnly, doc.DocumentSource))
        };

        _logger.LogInformation("Calling post index with values. Request body: {request}", JsonSerializer.Serialize(request));

        return await _client.PostJson<PostindexDocumentsRequest, PostindexDocumentsResponse, PostIndexError>(
             "v1/document-index/member-case",
             request,
             await GetAuthorizationHeader());
    }

    public async Task<Either<PostIndexError, PostindexDocumentsResponse>> PostIndexBereavementDocuments(
        string businessGroup,
        string caseNumber,
        IList<UploadedDocument> documents)
    {
        var request = new PostindexDocumentsRequest
        {
            BusinessGroup = businessGroup,
            Client = $"{businessGroup}1",
            CaseNumber = caseNumber,
            Documents = documents.Select(doc => new PostindexDocumentRequest(doc.Uuid, doc.Tags?.Split(';').ToList() ?? new List<string>(), doc.IsEdoc, doc.IsEpaOnly, doc.DocumentSource))
        };

        _logger.LogInformation("Calling post index with values. Request body: {request}", JsonSerializer.Serialize(request));

        return await _client.PostJson<PostindexDocumentsRequest, PostindexDocumentsResponse, PostIndexError>(
            "v1/document-index/non-member",
            request,
            await GetAuthorizationHeader());
    }

    public async Task<Either<PostIndexError, IndexResponse>> IndexRetirementDocument(
        string businessGroup,
        string referenceNumber,
        int batchNumber,
        string caseNumber,
        string caseCode)
    {
        return await _client.PostJson<IndexRequest, IndexResponse, PostIndexError>(
            "v1/postindex",
            new IndexRequest
            {
                BusinessGroup = businessGroup,
                ReferenceNumber = referenceNumber,
                CaseNumber = caseNumber,
                BatchNumber = batchNumber,
                CaseCode = caseCode,
            },
            await GetAuthorizationHeader());
    }

    public async Task<Either<PostIndexError, IndexResponse>> IndexBereavementDocument(
        string businessGroup,
        int batchNumber,
        string caseNumber)
    {
        return await _client.PostJson<IndexRequest, IndexResponse, PostIndexError>(
            "v1/postindex",
            new IndexRequest
            {
                BusinessGroup = businessGroup,
                ReferenceNumber = null,
                CaseNumber = caseNumber,
                BatchNumber = batchNumber,
                CaseCode = "NDD9",
                DocumentId = "ASSDNOT",
                NonMemberDocument = true
            },
            await GetAuthorizationHeader());
    }

    public async Task<Either<PostIndexError, IndexResponse>> IndexDocument(
        string businessGroup,
        string referenceNumber,
        int batchNumber)
    {
        return await _client.PostJson<IndexRequest, IndexResponse, PostIndexError>(
            "v1/postindex",
            new IndexRequest
            {
                BusinessGroup = businessGroup,
                ReferenceNumber = referenceNumber,
                BatchNumber = batchNumber,
                DocumentId = "TRNQU",
            },
            await GetAuthorizationHeader());
    }

    private async Task<(string, string)> GetAuthorizationHeader()
    {
        var token = (await _client.PostJson<LoginRequest, AccessTokenResponse>(
            "/v1/login",
            new LoginRequest { Password = _password, UserName = _userName })).AccessToken;

        return ("Authorization", $"JWT {token}");
    }

    private async Task<Stream> GetDocument(int id, (string Key, string Value) authorizationHeader)
    {
        var response = await _retryPolicy.ExecuteAsync(
            () => _client.Get($"/v1/images/{id}", authorizationHeader));

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync();
    }

    public async Task<Either<PostIndexError, PostindexDocumentsResponse>> IndexNonCaseDocuments(
       string businessGroup,
       string referenceNumber,
       IList<UploadedDocument> documents)
    {
        var request = new PostindexDocumentsRequest
        {
            BusinessGroup = businessGroup,
            Client = $"{businessGroup}1",
            ReferenceNumber = referenceNumber,
            Documents = documents.Select(doc => new PostindexDocumentRequest(doc.Uuid, doc.Tags?.Split(';').ToList() ?? new List<string>(), doc.IsEdoc, doc.IsEpaOnly, doc.DocumentSource))
        };

        _logger.LogInformation("Calling post index with values. Request body: {request}", JsonSerializer.Serialize(request));

        return await _client.PostJson<PostindexDocumentsRequest, PostindexDocumentsResponse, PostIndexError>(
             "v1/document-index/member",
             request,
             await GetAuthorizationHeader());
    }

    public async Task<Either<DocumentUploadError, DocumentUploadResponse>> UploadDocumentBase64(
      string businessGroup,
      string fileName,
      string blob,
      int? batchNumber = null)
    {
        return await _client.PostJson<DocumentUploadRequest, DocumentUploadResponse, DocumentUploadError>(
            "v1/document/bgroup",
            new DocumentUploadRequest
            {
                BusinessGroup = businessGroup,
                DocumentName = fileName,
                FileBlob = blob
            },
            await GetAuthorizationHeader());
    }
}