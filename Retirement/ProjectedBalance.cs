namespace WTW.MdpService.Retirement;

public record ProjectedBalance
{
    public string Age { get; init; }
    public decimal? AssetValue { get; init; }
}