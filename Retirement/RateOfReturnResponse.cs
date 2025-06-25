namespace WTW.MdpService.Retirement;

public record RateOfReturnResponse
{
    public decimal? personalRateOfReturn { get; set; }
    public decimal? changeInValue { get; set; }
}
