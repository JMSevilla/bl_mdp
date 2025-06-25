using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.TransferJourneys;

public class TransferValuesRequest
{
    [Required]
    public decimal RequestedResidualPension { get; set; }
}
