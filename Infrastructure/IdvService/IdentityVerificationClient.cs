using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WTW.MdpService.IdentityVerification;
using WTW.MdpService.Infrastructure.TokenService;
using WTW.Web;
using WTW.Web.Clients;

namespace WTW.MdpService.Infrastructure.IdvService;

public class IdentityVerificationClient : IIdentityVerificationClient
{
    private readonly HttpClient _client;
    private readonly ICachedTokenServiceClient _cachedTokenServiceClient;
    private readonly IdvServiceOptions _options;
    private readonly ILogger<IdentityVerificationClient> _logger;

    public IdentityVerificationClient(HttpClient client,
        ICachedTokenServiceClient cachedTokenServiceClient,
        IOptionsSnapshot<IdvServiceOptions> options,
        ILogger<IdentityVerificationClient> logger)
    {
        _client = client;
        _cachedTokenServiceClient = cachedTokenServiceClient;
        _options = options.Value;
        _logger = logger;
    }
    public async Task<VerifyIdentityResponse> VerifyIdentity(string businessGroup, string referenceNumber, VerifyIdentityRequest payload)
    {
        var token = await GetAccessToken();

        try
        {
            return await _client.PostJson<VerifyIdentityRequest, VerifyIdentityResponse>(
                string.Format(_options.VerifyIdentityAbsolutePath, businessGroup, referenceNumber),
                payload,
                (MdpConstants.AuthorizationHeaderName,
                $"{token}"));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError("{ActionName}, BusinessGroup: {BusinessGroup}, ReferenceNumber: {ReferenceNumber} Exception: {ex}",
                nameof(VerifyIdentity), businessGroup, referenceNumber, ex);
            return null;
        }
    }
    public async Task<UpdateIdentityResultResponse> SaveIdentityVerification(string businessGroup, string referenceNumber, SaveIdentityVerificationRequest payload)
    {
        var token = await GetAccessToken();
        try
        {
            return await _client.PutJson<SaveIdentityVerificationRequest, UpdateIdentityResultResponse>(
                string.Format(_options.SaveIdentityVerificationAbsolutePath, businessGroup, referenceNumber),
                payload,
                (MdpConstants.AuthorizationHeaderName,
                $"{token}"));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError("{ActionName}, BusinessGroup: {BusinessGroup}, ReferenceNumber: {ReferenceNumber} Exception: {ex}",
                nameof(SaveIdentityVerification), businessGroup, referenceNumber, ex);
            return null;
        }
    }
    private async Task<string> GetAccessToken()
    {
        return "Bearer " + (await _cachedTokenServiceClient.GetAccessToken()).AccessToken;
    }
}
