using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.TransferJourneys;

public record TransferJourneyContactRequest
{
    [MaxLength(50)]
    public string Name { get; init; }

    [MaxLength(50)]
    public string AdvisorName { get; init; }

    [MaxLength(50)]
    public string CompanyName { get; init; }

    [MaxLength(50)]
    public string Email { get; init; }

    [MaxLength(5)]
    public string PhoneCode { get; init; }

    [MaxLength(20)]
    public string PhoneNumber { get; init; }

    [Required]
    [MaxLength(50)]
    public string Type { get; init; }

    [MaxLength(50)]
    public string SchemeName { get; init; }
}