using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Infrastructure.Calculations;

public class CalculationServiceOptions
{
    [Required]
    public string BaseUrl { get; set; }
    [Required]
    public int TimeOutInSeconds { get; set; }
    [Required]
    public int CacheExpiresInMs { get; set; }
    [Required]
    public IEnumerable<string> GuaranteedQuotesEnabledFor { get; set; }
    [Required]
    public string GetGuaranteedQuotesApiPath { get; set; }
}
