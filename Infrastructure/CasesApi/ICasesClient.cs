using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;

namespace WTW.MdpService.Infrastructure.CasesApi;

public interface ICasesClient
{
    Task<Either<CreateCaseError, CreateCaseResponse>> CreateForMember(CreateCaseRequest request);
    Task<Either<CreateCaseError, CreateCaseResponse>> CreateForNonMember(CreateCaseRequest request);
    Task<Either<CreateCaseError, CaseExistsResponse>> Exists(string businessGroup, string caseNumber);
    Task<Either<DocumentsErrorResponse, DocumentsResponse>> ListDocuments(string businessGroup, string caseNumber);
    Task<Either<CasesErrorResponse, IEnumerable<CasesResponse>>> GetCaseList(string businessGroup, string referenceNumber);
    Task<Option<IEnumerable<CasesResponse>>> GetRetirementOrTransferCases(string businessGroup, string referenceNumber);
}
