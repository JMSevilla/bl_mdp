using System;
using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.TransferJourneys;

public record SubmitTransferFinancialAdviseRequest
{
    [Required]
    public DateTimeOffset? FinancialAdviseDate { get; init; }
}