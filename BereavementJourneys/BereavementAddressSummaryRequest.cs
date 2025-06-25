using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.BereavementJourneys;

public class BereavementAddressSummaryRequest
{
    [Required]
    public string Text { get; init; }
    public string Container { get; init; }
    public string Language { get; init; }
    public string Countries { get; init; }
}