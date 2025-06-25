using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Infrastructure.MemberWebInteractionService;

public class MemberWebInteractionServiceOptions
{
    [Required]
    public string? BaseUrl { get; set; }
    [Required]
    public string? GetEngagementEventsPath { get; set; }
    [Required]
    public string? GetMessagesPath { get; set; }
}
