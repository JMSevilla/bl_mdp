namespace WTW.MdpService.Retirement;
public record OptionsResponse
{
    public bool IsCalculationSuccessful { get; init; }
    public decimal? FullPensionYearlyIncome { get; init; }
    public decimal? MaxLumpSum { get; init; }
    public decimal? MaxLumpSumYearlyIncome { get; init; }

    public static OptionsResponse From(
        decimal? fullPensionYearlyIncome,
        decimal? maxLumpSum,
        decimal? maxLumpSumYearlyIncome)
    {
        return new()
        {
            IsCalculationSuccessful = true,
            FullPensionYearlyIncome = fullPensionYearlyIncome,
            MaxLumpSum = maxLumpSum,
            MaxLumpSumYearlyIncome = maxLumpSumYearlyIncome,
        };
    }

    public static OptionsResponse CalculationFailed()
    {
        return new()
        {
            IsCalculationSuccessful = false
        };
    }
}