using System;
using System.Net.Http;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WTW.MdpService.Infrastructure.TokenService;
using WTW.Web.Clients;

namespace WTW.MdpService.Infrastructure.IpaService;

public class IpaServiceClient : IIpaServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ICachedTokenServiceClient _cachedTokenServiceClient;
    private readonly IpaServiceOptions _ipaServiceOptions;
    private readonly ILogger<IpaServiceClient> _logger;

    public IpaServiceClient(HttpClient httpClient,
        ICachedTokenServiceClient cachedTokenServiceClient,
        IOptionsSnapshot<IpaServiceOptions> options,
        ILogger<IpaServiceClient> logger)
    {
        _httpClient = httpClient;
        _cachedTokenServiceClient = cachedTokenServiceClient;
        _ipaServiceOptions = options.Value;
        _logger = logger;
    }
    public async Task<Option<GetCountriesResponse>> GetCountries()
    {
        try
        {
            return await _httpClient.GetJson<GetCountriesResponse>(
                _ipaServiceOptions.GetCountriesAbsolutePath,
                ("Authorization", $"{await GetAccessToken()}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCountries - returned error");
            return Option<GetCountriesResponse>.None;
        }
    }
    public async Task<Option<GetCurrenciesResponse>> GetCurrencies()
    {
        try
        {
            return await _httpClient.GetJson<GetCurrenciesResponse>(
                _ipaServiceOptions.GetCurrenciesAbsolutePath,
                ("Authorization", $"{await GetAccessToken()}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCurrencies - returned error");
            return Option<GetCurrenciesResponse>.None;
        }
    }
    private async Task<string> GetAccessToken()
    {
        return "Bearer " + (await _cachedTokenServiceClient.GetAccessToken()).AccessToken;
    }
}
