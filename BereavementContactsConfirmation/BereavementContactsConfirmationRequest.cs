using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.BereavementContactsConfirmation;

public record BereavementContactsConfirmationRequest
{
    [Required]
    [MaxLength(50)]
    public string EmailAddress { get; init; }

    [Required]
    public string ContentAccessKey { get; init; }
}

