using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.Infrastructure.Journeys.Documents;

public interface IJourneyDocumentsHandlerService
{
    Task<Either<Error, Unit>> PostIndex(string businessGroup, string referenceNumber, string caseNumber, string journeyType);
}