using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Infrastructure.Investment.AnnuityBroker;

namespace WTW.MdpService.Infrastructure.Investment;

public interface IInvestmentQuoteService
{
    Task<Either<Error, InvestmentQuoteRequest>> CreateAnnuityQuoteRequest(string businessGroup, string referenceNumber, RetirementV2 retirementV2);
}