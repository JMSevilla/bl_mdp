using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Journeys.Start;

public record StartDcJourneyRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(25)]
    public string CurrentPageKey { get; init; }

    [Required]
    [MinLength(2)]
    [MaxLength(25)]
    public string NextPageKey { get; init; }

    public string JourneyStatus { get; init; }
    public string SelectedQuoteName { get; init; }
}