using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.TransferJourneys;

public record SendIfaEmailsRequest
{
    [Required]
    public string ContentAccessKey { get; init; }
}