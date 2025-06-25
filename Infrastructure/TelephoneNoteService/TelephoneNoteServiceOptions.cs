#nullable enable
using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Infrastructure.TelephoneNoteService;

public class TelephoneNoteServiceOptions
{
    [Required]
    public string? BaseUrl { get; set; }
    
    [Required]
    public string? GetIntentContextAbsolutePath { get; set; }
}
#nullable disable 