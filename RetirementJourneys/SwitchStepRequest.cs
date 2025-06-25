using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.RetirementJourneys;

public record SwitchStepRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(25)]
    public string SwitchPageKey { get; init; }


    [Required]
    [MinLength(2)]
    [MaxLength(25)]
    public string NextPageKey { get; init; }
}