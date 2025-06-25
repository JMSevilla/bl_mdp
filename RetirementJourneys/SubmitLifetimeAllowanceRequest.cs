using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.RetirementJourneys;

public record SubmitLifetimeAllowanceRequest
{
    [Required]
    [Range(0.01, 999.99)]
    public decimal Percentage { get; init; }
}