using WTW.MdpService.Infrastructure.Calculations;

namespace WTW.MdpService.Retirement;

public class RecalculatedLumpSumResponse
{
    public RecalculatedLumpSumResponse() { }

    public RecalculatedLumpSumResponse(MdpResponseV2 mdp)
    {
        IsCalculationSuccessful = true;
        Quotes = mdp;
    }

    public bool IsCalculationSuccessful { get; init; }
    public MdpResponseV2 Quotes { get; init; }

    public RecalculatedLumpSumResponse CalculationFailed() => new() { IsCalculationSuccessful = false };
}