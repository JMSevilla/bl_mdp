using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Infrastructure.EngagementEvents;

namespace WTW.MdpService.Infrastructure.MemberWebInteractionService;

public interface IMemberWebInteractionServiceClient
{
    Task<Option<MemberWebInteractionEngagementEventsResponse>> GetEngagementEvents(string businessGroup,
                                                                                   string referenceNumber);

    Task<Option<MemberMessagesResponse>> GetMessages(string businessGroup, string referenceNumber);
}

