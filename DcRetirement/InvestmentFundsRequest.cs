using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.DcRetirement;

public class InvestmentFundsRequest
{
    [Required]
    public string SchemeCode { get; set; }
    [Required]
    public string Category { get; set; }
}
