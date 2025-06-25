using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Journeys;

public record StartJourneyRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(25)]
    public string CurrentPageKey { get; init; }

    [Required]
    [MinLength(2)]
    [MaxLength(25)]
    public string NextPageKey { get; init; }

    public bool RemoveOnLogin { get; init; }
    public string JourneyStatus { get; init; }
}