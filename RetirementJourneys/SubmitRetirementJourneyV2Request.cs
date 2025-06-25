using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.RetirementJourneys;

public record SubmitRetirementJourneyV2Request
{
    [Required]
    public string ContentAccessKey { get; init; }

    [Required]
    public bool Acknowledgement { get; init; }

    [Required]
    public bool AcknowledgementPensionWise { get; init; }

    [Required]
    public bool AcknowledgementFinancialAdvisor { get; init; }
}