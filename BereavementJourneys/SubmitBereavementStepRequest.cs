using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.BereavementJourneys;

public record SubmitBereavementStepRequest
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