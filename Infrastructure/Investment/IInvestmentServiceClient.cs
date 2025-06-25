using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using WTW.MdpService.DcRetirement;
using WTW.MdpService.Infrastructure.Investment.AnnuityBroker;

namespace WTW.MdpService.Infrastructure.Investment;

public interface IInvestmentServiceClient
{
    Task<Option<InvestmentInternalBalanceResponse>> GetInternalBalance(string referenceNumber, string businessGroup, string schemeType);
    Task<Either<Error, InvestmentForecastResponse>> GetInvestmentForecast(string referenceNumber, string businessGroup, int targetAge, string schemeType);
    Task<Option<InvestmentForecastResponse>> GetInvestmentForecast(string referenceNumber, string businessGroup, IEnumerable<int> targetAges);
    Task<Option<InvestmentForecastAgeResponse>> GetInvestmentForecastAge(string referenceNumber, string businessGroup);
    Task<Option<DcSpendingResponse<FundContributionTypeResponse>>> GetInvestmentFunds(string businessGroup, string schemeCode, string contType);
    Task<Option<DcSpendingResponse<StrategyContributionTypeResponse>>> GetInvestmentStrategies(string businessGroup, string schemeCode, string contType);
    Task<Option<LatestContributionResponse>> GetLatestContribution(string businessGroup, string referenceNumber);
    Task<Option<TargetSchemeMappingResponse>> GetTargetSchemeMappings(string businessGroup, string schemeCode, string category);
    Task<Either<Error, Unit>> CreateAnnuityQuote(string businessGroup, string referenceNumber, InvestmentQuoteRequest request);
}