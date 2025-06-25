using System.ComponentModel.DataAnnotations;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.TransferJourneys;

public record TransferApplicationStatusRequest
{
    [Required]
   public TransferApplicationStatus Status { get; set; }
}
