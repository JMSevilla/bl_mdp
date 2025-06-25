using LanguageExt;
using WTW.MdpService.Infrastructure.Investment;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Investment;

public class InternalBalanceResponse
{
    public InternalBalanceResponse(Option<InvestmentInternalBalanceResponse> investmentInternalBalance)
    {
        DcBalance = investmentInternalBalance.IsSome ? investmentInternalBalance.Value().TotalValue : null;
    }

    public decimal? DcBalance { get; set; }
}
