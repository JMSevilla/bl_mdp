using System.Threading.Tasks;
using LanguageExt;
using WTW.Web;

namespace WTW.MdpService.Infrastructure.TelephoneNoteService;

public interface ITelephoneNoteServiceClient
{
    Task<Option<IntentContextResponse>> GetIntentContext(string businessGroup, string referenceNumber, string platform = MdpConstants.AppPlatform);
} 