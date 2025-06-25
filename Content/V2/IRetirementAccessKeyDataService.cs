using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Calculations;

namespace WTW.MdpService.Content.V2;

public interface IRetirementAccessKeyDataService
{
    Task<Either<Error, Calculation>> GetNewRetirementCalculation(RetirementDatesAgesResponse retirementDatesAgesResponse, Member member);
    Task<Either<Error, Calculation>> GetRetirementCalculationWithJourney(
        RetirementDatesAgesResponse retirementDatesAgesResponse,
        string referenceNumber,
        string businessGroup);
    Task<ExistingRetirementJourneyType> GetExistingRetirementJourneyType(Member member);
    Task<Option<Calculation>> GetRetirementCalculation(string referenceNumber, string businessGroup);

    RetirementApplicationStatus GetRetirementApplicationStatus(Member member,
        Either<Error, Calculation> retirementCalculation,
        int preRetirementAgePeriodInYears,
        int newlyRetiredRangeInMonth);
    Task UpdateRetirementDatesAges(Calculation calculation, RetirementDatesAgesResponse retirementDatesAgesResponse);
}