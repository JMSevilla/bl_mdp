using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.DcRetirement;

public class InvestmentStrategiesRequest
{
    [Required]
    public string SchemeCode { get; set; }
    [Required]
    public string Category { get; set; }
}
