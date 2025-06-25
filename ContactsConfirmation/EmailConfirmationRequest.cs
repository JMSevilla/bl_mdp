using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.ContactsConfirmation;

public record EmailConfirmationRequest
{
    [Required]
    [MinLength(6)]
    [MaxLength(6)]
    public string EmailConfirmationToken { get; init; }
}