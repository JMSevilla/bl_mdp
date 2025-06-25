using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.TransferJourneys;

public record StartTransferJourneyRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(25)]
    public string CurrentPageKey { get; init; }

    [Required]
    [MinLength(2)]
    [MaxLength(25)]
    public string NextPageKey { get; init; }

    public string ContentAccessKey { get; init; }
}