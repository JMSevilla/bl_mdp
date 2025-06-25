using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Extensions.Logging;
using WTW.Web.Clients;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Infrastructure.CasesApi;

public class CasesClient : ICasesClient
{
    private readonly HttpClient _client;
    private readonly ILogger<CasesClient> _logger;

    public CasesClient(HttpClient client, ILogger<CasesClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Either<CreateCaseError, CreateCaseResponse>> CreateForNonMember(CreateCaseRequest request)
    {
        return await _client.PostJson<CreateCaseRequest, CreateCaseResponse, CreateCaseError>(
            "api/v1.0/create/non-member-case",
            request);
    }

    public async Task<Either<CreateCaseError, CreateCaseResponse>> CreateForMember(CreateCaseRequest request)
    {
        return await _client.PostJson<CreateCaseRequest, CreateCaseResponse, CreateCaseError>(
            "api/v1.0/create/member-case",
            request);
    }

    public async Task<Either<CreateCaseError, CaseExistsResponse>> Exists(string businessGroup, string caseNumber)
    {
        return await _client.GetJson<CaseExistsResponse, CreateCaseError>($"api/v1.0/case-exists/bgroup/{businessGroup}/caseno/{caseNumber}");
    }

    public async Task<Either<DocumentsErrorResponse, DocumentsResponse>> ListDocuments(string businessGroup, string caseNumber)
    {
        return await _client.GetJson<DocumentsResponse, DocumentsErrorResponse>($"api/v1.0/bgroup/{businessGroup}/caseno/{caseNumber}/document");
    }

    public async Task<Either<CasesErrorResponse, IEnumerable<CasesResponse>>> GetCaseList(string businessGroup, string referenceNumber)
    {
        return await _client.GetJson<IEnumerable<CasesResponse>, CasesErrorResponse>($"api/v1.0/bgroups/{businessGroup}/members/{referenceNumber}/cases");
    }

    public async Task<Option<IEnumerable<CasesResponse>>> GetRetirementOrTransferCases(string businessGroup, string referenceNumber)
    {
        try
        {
            var response = await _client.GetJson<IEnumerable<CasesResponse>, CasesErrorResponse>($"api/v1.0/bgroups/{businessGroup}/members/{referenceNumber}/cases?casecode=RTQ9&casecode=TOQ9&casecode=RTP9&casecode=TOP9");

            if (response.IsLeft)
            {

                _logger.LogWarning("Cases not found or bad request for member {referenceNumber} in business group {businessGroup}. Error: {message}. Detail {detail}", referenceNumber, businessGroup, response.Left().Message, response.Left().Detail);
                return Option<IEnumerable<CasesResponse>>.None;
            }

            return Option<IEnumerable<CasesResponse>>.Some(response.Right());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting cases for member {referenceNumber} in business group {businessGroup}.", referenceNumber, businessGroup);
            return Option<IEnumerable<CasesResponse>>.None;
        }
    }
}