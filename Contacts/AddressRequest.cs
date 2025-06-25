using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Contacts;

public record AddressRequest
{
    [Required]
    [MaxLength(50)]
    public string StreetAddress1 { get; init; }

    [MaxLength(50)]
    public string StreetAddress2 { get; init; }

    [MaxLength(50)]
    public string StreetAddress3 { get; init; }

    [MaxLength(50)]
    public string StreetAddress4 { get; init; }

    [MaxLength(50)]
    public string StreetAddress5 { get; init; }

    [MaxLength(30)]
    public string Country { get; init; }

    [Required]
    [MaxLength(3)]
    public string CountryCode { get; init; }

    [MaxLength(8)]
    public string PostCode { get; init; }
}