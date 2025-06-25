using System.ComponentModel.DataAnnotations;
using System;

namespace WTW.MdpService.TransferJourneys;

public record SubmitTransferPensionWiseRequest
{
    [Required]
    public DateTimeOffset? PensionWiseDate { get; init; }
}