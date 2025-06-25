using System.ComponentModel.DataAnnotations;
using WTW.MdpService.Domain.Common.Journeys;

namespace WTW.MdpService.Documents;

public class JourneyDocumentDeleteAllRequest
{
    [Required]
    public string JourneyType { get; set; }
}
