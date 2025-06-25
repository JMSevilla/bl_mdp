using System;

namespace WTW.MdpService.Retirement;

public class LatestProtectedQuoteResponse
{
    public DateTime? QuoteExpiryDate { get; set; }
    public DateTime? QuoteRetirementDate { get; set; }
    public string AgeAtRetirementDateIso { get; set; }
    public decimal? TotalPension { get; set; }
    public decimal? TotalAVCFund { get; set; }
    public bool IsCalculationSuccessful { get; init; } = true;

    public static LatestProtectedQuoteResponse CalculationFailed()
    {
        return new()
        {
            IsCalculationSuccessful = false
        };
    }

} 