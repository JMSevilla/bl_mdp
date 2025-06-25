using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.IpaService;

public class GetCurrenciesResponse
{
    public List<CurrencyDetails> Currencies { get; set; }
}
public class CurrencyDetails
{
    public string CurrencyCode { get; set; }
    public string CurrencyName { get; set; }
    public string CountryCode { get; set; }
}
