using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Journeys.JourneysGenericData;

public record SaveJourneyGenericDataRequest
{
    [Required]
    public string GenericDataJson { get; init; }
}