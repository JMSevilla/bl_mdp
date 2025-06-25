using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Journeys.Submit.Services.Dto;

namespace WTW.MdpService.Journeys.Submit.Services;

public interface IGenericJourneyService
{
    Task SaveSubmissionDetailsToGenericData(string businessGroup, string referenceNumber, string journeyType, (string CaseNumber, int EdmsImageId) details);
    Task SetStatusSubmitted(string businessGroup, string referenceNumber, string journeyType);
    Task<Either<Error, SubmissionDetailsDto>> GetSubmissionDetailsFromGenericData(string businessGroup, string referenceNumber, string journeyType);
    Task<GenericJourney> CreateJourney(string businessGroup, string referenceNumber, string journeyType, string currentPageKey, string nextPageKey, bool removeOnLogin, string journeyStatus);
    Task<bool> ExistsJourney(string businessGroup, string referenceNumber, string journeyType);
    Task UpdateDcRetirementSelectedJourneyQuoteDetails(Calculation calculation);
}