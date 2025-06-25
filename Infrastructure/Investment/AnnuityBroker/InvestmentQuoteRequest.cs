namespace WTW.MdpService.Infrastructure.Investment.AnnuityBroker;

public class InvestmentQuoteRequest
{
    public string EventType { get; set; }
    public string AutomatedInd { get; set; }
    public string FullDataSet { get; set; }
    public string MemberRequestedInd { get; set; }
    public string ManualInputStatus { get; set; }
    public InvestmentQuoteDetails Quote { get; set; }
    public InvestmentQuoteMemberSummary Member { get; set; }
}