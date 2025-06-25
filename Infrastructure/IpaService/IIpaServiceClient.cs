using System.Threading.Tasks;
using LanguageExt;

namespace WTW.MdpService.Infrastructure.IpaService;

public interface IIpaServiceClient
{
    Task<Option<GetCountriesResponse>> GetCountries();
    Task<Option<GetCurrenciesResponse>> GetCurrencies();
}
