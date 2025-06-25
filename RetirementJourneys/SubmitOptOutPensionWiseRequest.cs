using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.RetirementJourneys;

public record SubmitOptOutPensionWiseRequest
{
    [Required]
    public bool OptOutPensionWise { get; init; } 
}