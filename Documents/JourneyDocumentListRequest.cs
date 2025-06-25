using System.ComponentModel.DataAnnotations;
using WTW.MdpService.Domain.Common.Journeys;

namespace WTW.MdpService.Documents;

public class JourneyDocumentListRequest
{
    [Required]
    public string JourneyType { get; set; }
}
