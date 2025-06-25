using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.ContactsConfirmation;

public record MobilePhoneConfirmationRequest
{
    [Required]
    [MinLength(6)]
    [MaxLength(6)]
    public string MobilePhoneConfirmationToken { get; init; }

    [Required]
    [MinLength(1)]
    [MaxLength(80)]
    public string MobilePhoneCountry { get; init; }
}