using WTW.MdpService.Domain.Investment;

namespace WTW.MdpService.Infrastructure.Investment;

public class InvestmentMappingService
{
    public InvestmentInternalBalance MapResponseToDomain(InvestmentInternalBalanceResponse response)
    {
        var contributions = response.Contributions.Map(x => new InvestmentContributionsData(x.Code, x.Name, x.PaidIn, x.Value));
        var investmentFunds = response.Funds.Map(x => new InvestmentFund(x.Code, x.Name, x.Value));
        return new InvestmentInternalBalance(response.Currency, response.TotalPaidIn, response.TotalValue, contributions, investmentFunds);
    }
}
