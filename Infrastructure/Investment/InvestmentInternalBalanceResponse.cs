using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.Investment;

public class InvestmentInternalBalanceResponse
{
    public string Currency { get; set; }
    public decimal TotalPaidIn { get; set; }
    public decimal TotalValue { get; set; }
    public List<InvestmentContributionResponse> Contributions { get; set; }
    public List<InvestmentFundResponse> Funds { get; set; }
}