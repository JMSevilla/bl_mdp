using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Addresses;

public record AddressSummaryRequest
{
    [Required]
    public string Text { get; init; }
    public string Container { get; init; }
    public string Language { get; init; }
    public string Countries { get; init; }
}