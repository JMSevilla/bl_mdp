using System;
using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Journeys.Start;

public record StartDbCoreJourneyRequest
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

    [Required]
    public DateTime RetirementDate { get; set; }
}