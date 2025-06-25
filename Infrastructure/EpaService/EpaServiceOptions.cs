using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Infrastructure.EpaService;

public class EpaServiceOptions
{
    [Required]
    public string BaseUrl { get; set; }

    [Required]
    public string GetWebRuleAbsolutePath { get; set; }
    [Required]
    public string GetEpaUserAbsolutePath { get; set; }
}
