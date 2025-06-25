using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Infrastructure.DeloreanAuthentication;

public class DeloreanAuthenticationOptions
{
    [Required]
    public string BaseUrl { get; set; }
    [Required]
    public string GetMemberAbsolutePath { get; set; }
    [Required]
    public string UpdateMemberAbsolutePath { get; set; }
    [Required]
    public string CheckEligibilityAbsolutePath { get; set; }
    [Required]
    public string RegisterRelatedMemberAbsolutePath { get; set; }
    [Required]
    public string GenerateTokenPath { get; set; }
}
