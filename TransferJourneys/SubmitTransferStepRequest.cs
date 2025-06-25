using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.TransferJourneys;

public record SubmitTransferStepRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(25)]
    public string CurrentPageKey { get; init; }

    [Required]
    [MinLength(2)]
    [MaxLength(25)]
    public string NextPageKey { get; init; }
}