using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.Investment;

namespace WTW.MdpService.DcRetirement;

public record LifeSighRetirementDateAgeResponse
{
    public LifeSighRetirementDateAgeResponse(InvestmentForecastAgeResponse response)
    {
        DcRetirementDate = response.RetirementDate;
        DcRetirementAge = response.RetirementAge;
    }

    public string DcRetirementDate { get; init; }
    public int DcRetirementAge { get; init; }
}