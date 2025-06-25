using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Domain.Common.Journeys;

namespace WTW.MdpService.Infrastructure.Journeys;

public interface IGenericJourneyDetails
{
    Task<Option<GenericJourneyData>> GetAll(string businessGroup, string referenceNumber, string journeyType);
    GenericJourneyData GetAll(GenericJourney genericJourney);
}