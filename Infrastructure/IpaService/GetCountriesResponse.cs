using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.IpaService;

public class GetCountriesResponse
{
    public List<CountryDetails> Countries { get; set; }
}

public class CountryDetails
{
    public string CountryCode { get; set; }
    public string CountryName { get; set; }
}
