using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.ContactsConfirmation;

public record SendEmailConfirmationV2Request
{
    [Required]
    [MaxLength(50)]
    public string EmailAddress { get; init; }

    [Required]
    public string ContentAccessKey { get; init; }
}