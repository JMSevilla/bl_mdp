using System.Net.Http;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using WTW.Web.Clients;
using WTW.Web.Extensions;

namespace WTW.MdpService.Infrastructure.Geolocation;

public class LoqateApiClient : ILoqateApiClient
{
    private readonly ILogger<LoqateApiClient> _logger;
    private readonly HttpClient _httpClient;
    private readonly LoqateApiConfiguration _configuration;

    public LoqateApiClient(HttpClient httpClient, LoqateApiConfiguration configuration,
        ILogger<LoqateApiClient> logger)
    {
        _logger = logger;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<Either<Error, LocationApiAddressSummaryResponse>> Find(string text, string container, string language, string countries)
    {
        var queryString = new 
        {
            Key = _configuration.ApiKey, 
            Text = text,
            Bias = false,
            IsMiddleware = true, 
            Container = container,
            Language = language,
            Countries = countries
        }.ToQueryString();
 
        var result =
            await _httpClient.GetJson<LocationApiAddressSummaryResponse>(
                $"Capture/Interactive/Find/v1.1/json3.ws?{queryString}");

        if (!result.IsSuccess)
            _logger.LogError(string.Join(", ", result.Errors));

        return !result.IsSuccess ? Error.New("Bad request") : result;
    }

    public async Task<Either<Error, LocationApiAddressDetailsResponse>> GetDetails(
        string addressId)
    {
        var queryString = new
        {
            Key = _configuration.ApiKey, 
            Id = addressId
        }.ToQueryString();

        var result =
            await _httpClient.GetJson<LocationApiAddressDetailsResponse>(
                $"Capture/Interactive/Retrieve/v1.2/json3.ws?{queryString}");

        if (!result.IsSuccess)
            _logger.LogError(string.Join(", ", result.Errors));

        return !result.IsSuccess ? Error.New("Bad request") : result;
    }
}