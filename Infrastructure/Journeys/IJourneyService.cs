using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.Infrastructure.Journeys;

public interface IJourneyService
{
    Task<Option<Journey>> GetJourney(string journeyType, string businessGroup, string referenceNumber);
    Task<Option<RetirementJourney>> FindUnexpiredOrSubmittedJourney(string businessGroup, string referenceNumber);
}
