namespace WTW.MdpService.Retirement;

public record RetirementCalculationResponse
{
    public bool IsCalculationSuccessful { get; init; }
    public decimal? TotalPension { get; init; }
    public decimal? TotalAVCFund { get; init; }

    public static RetirementCalculationResponse From(Domain.Mdp.Calculations.RetirementV2 retirement)
    {
        return new()
        {
            IsCalculationSuccessful = true,
            TotalPension = retirement.TotalPension(),
            TotalAVCFund = retirement.TotalAVCFund()
        };
    }

    public static RetirementCalculationResponse CalculationFailed()
    {
        return new()
        {
            IsCalculationSuccessful = false
        };
    }
}