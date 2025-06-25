using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.ContactsConfirmation;

public record SendMobilePhoneConfirmationV2Request
{
    [Required]
    [MaxLength(5)]
    public string Code { get; init; }

    [Required]
    [MaxLength(20)]
    public string Number { get; init; }

    [Required]
    public string ContentAccessKey { get; init; }
}