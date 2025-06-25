namespace WTW.MdpService.Infrastructure.Investment.AnnuityBroker;

public class InvestmentQuoteDetails
{
    public string QuoteType { get; set; }
    public int PclsAmount { get; set; }
    public string FundValue { get; set; }
    public string AvcFundValue { get; set; }
    public string MiscellaneousValue1 { get; set; }
    public string MiscellaneousValue2 { get; set; }
    public decimal? ResidualFundValue { get; set; }
    public string GmpFundValue { get; set; }
    public string MinimumPension1 { get; set; }
    public string MinimumPension2 { get; set; }
    public string ValueDate { get; set; }
    public string CalculationCode { get; set; }
}