using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Charts;

public class ChartsRequest
{
    [Required]
    public string TenantUrl { get; init; }
}
