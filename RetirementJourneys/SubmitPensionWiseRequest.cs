using System;
using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.RetirementJourneys;

public record SubmitPensionWiseRequest
{
    [Required]
    public DateTimeOffset? PensionWiseDate { get; init; }
}