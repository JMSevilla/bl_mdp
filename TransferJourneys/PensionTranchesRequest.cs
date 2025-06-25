using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.TransferJourneys;

public class PensionTranchesRequest
{
    [Required]
    public decimal RequestedTransferValue { get; set; }
}
