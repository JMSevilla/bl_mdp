using System.Threading.Tasks;
using LanguageExt.Common;

namespace WTW.MdpService.DcRetirement.Services;

public interface IDcRetirementService
{
    Task<Error?> ResetQuote(string referenceNumber, string businessGroup);
}