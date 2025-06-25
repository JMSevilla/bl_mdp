using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WTW.Web.Clients;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Infrastructure.TokenService;

public class TokenServiceClient : ICachedTokenServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly TokenServiceClientConfiguration _configuration;
    private readonly ILogger<TokenServiceClient> _logger;

    public TokenServiceClient(HttpClient httpClient, TokenServiceClientConfiguration configuration, ILogger<TokenServiceClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<TokenServiceResponse> GetAccessToken()
    {
        var response = await _httpClient.PostJson<TokenServiceResponse, TokenServiceErrorResponse, TokenServiceRequest>(
            "/api/v1/oauth2/token",
            new TokenServiceRequest(
                _configuration.GrantType,
                _configuration.ClientId,
                _configuration.ClientSecret,
                _configuration.Scopes ?? Enumerable.Empty<string>()));

        if (response.IsLeft)
            _logger.LogError("Failed to get access token from Token Service. Error message: {errorMessage}", response.Left().Message);

        return response.Right();
    }
}