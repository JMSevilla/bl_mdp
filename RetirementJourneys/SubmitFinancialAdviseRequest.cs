using System;
using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.RetirementJourneys;

public record SubmitFinancialAdviseRequest
{
    [Required]
    public DateTimeOffset? FinancialAdviseDate { get; init; }
}