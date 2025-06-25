using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.TransferJourneys;

public class TransferJourneySubmitRequest
{
    [Required]
    public string ContentAccessKey { get; init; }
}