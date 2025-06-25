using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using WTW.MdpService.Infrastructure.CasesApi;

namespace WTW.MdpService.Journeys.Submit.Services;

public interface ICaseRequestFactory
{
    CreateCaseRequest CreateForGenericRetirement(string businessGroup, string referenceNumber, string caseCode = "RTP9");
    Task<Either<Error, CreateCaseRequest>> CreateForQuoteRequest(string businessGroup, string referenceNumber, string caseType);
}