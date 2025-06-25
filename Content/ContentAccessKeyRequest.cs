using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Content;

public record ContentAccessKeyRequest
{
    [Required]
    public string TenantUrl { get; init; }
    public int PreRetirementAgePeriodInYears { get; init; }
    public int NewlyRetiredRangeInMonth { get; init; }
    public bool BasicMode { get; init; }
}