namespace WTW.MdpService.Infrastructure.Investment;

public record FundResponse
{
    public string Code { get; init; }
    public string Name { get; init; }
    public decimal? AnnualMemberFee { get; init; }
}