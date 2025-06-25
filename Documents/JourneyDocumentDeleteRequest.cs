using System.ComponentModel.DataAnnotations;
using WTW.MdpService.Domain.Common;

namespace WTW.MdpService.Documents;

public class JourneyDocumentDeleteRequest
{
    [Required]
    public string Uuid { get; set; }
}
