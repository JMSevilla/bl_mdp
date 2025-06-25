using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Infrastructure.IpaService;

public class IpaServiceOptions
{
    [Required]
    public string BaseUrl { get; set; }

    [Required]
    public string GetCountriesAbsolutePath { get; set; }
    [Required]
    public string GetCurrenciesAbsolutePath { get; set; }
}
