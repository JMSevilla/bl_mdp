using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Journeys.Submit;

public class SubmitDcJourneyRequest
{
    [Required]
    public string ContentAccessKey { get; init; }
}