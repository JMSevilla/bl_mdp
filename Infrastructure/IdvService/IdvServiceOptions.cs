#nullable enable
using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Infrastructure.IdvService;

public class IdvServiceOptions
{
    [Required]
    public string? BaseUrl { get; set; }
    [Required]
    public string? VerifyIdentityAbsolutePath { get; set; }
    [Required]
    public string? SaveIdentityVerificationAbsolutePath { get; set; }
}
#nullable disable
