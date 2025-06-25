using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Infrastructure.MemberService;

public class MemberServiceOptions
{
    [Required]
    public string BaseUrl { get; set; }
    [Required]
    public string GetBeneficiariesPath { get; set; }
    [Required]
    public string GetLinkedMemberPath { get; set; }
    [Required]
    public string GetMemberSummaryPath { get; set; }
    [Required]
    public string GetPensionDetailsPath { get; set; }
    [Required]
    public string GetPersonalDetailPath { get; set; }
    [Required]
    public string GetContactDetailsPath { get; set; }
    [Required]
    public string GetMemberMatchingAbsolutePath { get; set; }
}
