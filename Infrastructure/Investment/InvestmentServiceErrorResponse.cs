namespace WTW.MdpService.Infrastructure.Investment;

public record InvestmentServiceErrorResponse
{
    public string Message { get; set; }
    public int Code { get; set; }
}