using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using WTW.MdpService.Infrastructure.CasesApi;

namespace WTW.MdpService.Journeys.Submit.Services;

public interface ICaseService
{
    Task<Either<Error, string>> Create(CreateCaseRequest createCaseRequest);
}