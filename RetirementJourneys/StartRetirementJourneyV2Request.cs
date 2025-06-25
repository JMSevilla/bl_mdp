using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.RetirementJourneys;

public record StartRetirementJourneyV2Request
{
    [Required]
    [MinLength(2)]
    [MaxLength(25)]
    public string CurrentPageKey { get; init; }

    [Required]
    [MinLength(2)]
    [MaxLength(25)]
    public string NextPageKey { get; init; }

    [Required]
    public string SelectedQuoteName { get; init; }
}