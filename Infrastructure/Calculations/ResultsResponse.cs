namespace WTW.MdpService.Infrastructure.Calculations;

public record ResultsResponse
{
    public MdpResponseV2 Mdp { get; init; }
    public QuotationResponse Quotation { get; set; } = new QuotationResponse();
}