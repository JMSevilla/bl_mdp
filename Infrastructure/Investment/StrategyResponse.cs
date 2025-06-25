namespace WTW.MdpService.Infrastructure.Investment;

public record StrategyResponse
{
    public string Code { get; init; }
    public string Name { get; init; }
}