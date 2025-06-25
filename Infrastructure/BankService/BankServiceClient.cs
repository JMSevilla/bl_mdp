using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WTW.MdpService.Infrastructure.TokenService;
using WTW.Web;
using WTW.Web.Clients;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Infrastructure.BankService;

public class BankServiceClient : IBankServiceClient
{
    private readonly HttpClient _client;
    private readonly ICachedTokenServiceClient _cachedTokenServiceClient;
    private readonly BankServiceOptions _options;
    private readonly ILogger<BankServiceClient> _logger;

    public BankServiceClient(HttpClient client,
                             ICachedTokenServiceClient cachedTokenServiceClient,
                             IOptionsSnapshot<BankServiceOptions> options,
                             ILogger<BankServiceClient> logger)
    {
        _client = client;
        _cachedTokenServiceClient = cachedTokenServiceClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<HttpStatusCode> AddBankAccount(string bgroup, string refNo, AddBankAccountPayload payload)
    {
        var token = await GetAccessToken();

        var response = await _client.PostJson<AddBankAccountPayload>(
            string.Format(_options.PostBankAccountPath, bgroup, refNo),
            payload,
            (MdpConstants.AuthorizationHeaderName, $"{token}"));
        response.EnsureSuccessStatusCode();

        return response.StatusCode;
    }

    public async Task<Option<GetBankAccountClientResponse>> GetBankAccount(string bgroup, string refNo)
    {
        var token = await GetAccessToken();

        try
        {
            var result = await _client.GetOptionalJson<GetBankAccountClientResponse>(
               string.Format(_options.GetBankAccountPath, bgroup, refNo),
               (MdpConstants.AuthorizationHeaderName, $"{token}"));

            if (result.IsNone)
            {
                _logger.LogWarning("No bank account record found for {businessGroup} {referenceNumber}.", bgroup, refNo);
                return result;
            }

            return result.Value();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError("{ActionName}, BusinessGroup: {BusinessGroup}, ReferenceNumber: {ReferenceNumber} Exception: {ex}",
                nameof(GetBankAccount), bgroup, refNo, ex);
            return null;
        }
    }

    public async Task<ValidateBankAccountResponse> ValidateBankAccount(string bgroup, string refNo, ValidateBankAccountPayload payload)
    {
        var token = await GetAccessToken();

        try
        {
            return await _client.PostJson<ValidateBankAccountPayload, ValidateBankAccountResponse>(
                                string.Format(_options.ValidateBankAccountPath, bgroup, refNo),
                                payload,
                                (MdpConstants.AuthorizationHeaderName,
                                $"{token}"));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError("{ActionName}, BusinessGroup: {BusinessGroup}, ReferenceNumber: {ReferenceNumber} Exception: {ex}",
                nameof(ValidateBankAccount), bgroup, refNo, ex);
            return null;
        }
    }

    public async Task<VerifySafePaymentResponse> VerifySafePayment(string bgroup, string refNo, VerifySafePaymentPayload payload)
    {
        var token = await GetAccessToken();
        try
        {
            return await _client.PostJson<VerifySafePaymentPayload, VerifySafePaymentResponse>(
                string.Format(_options.VerifySafePaymentPath, bgroup, refNo),
                payload,
                (MdpConstants.AuthorizationHeaderName,
                $"{token}"));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError("{ActionName}, BusinessGroup: {BusinessGroup}, ReferenceNumber: {ReferenceNumber} Exception: {ex}",
                nameof(VerifySafePayment), bgroup, refNo, ex);
            return null;
        }
    }
    private async Task<string> GetAccessToken()
    {
        return "Bearer " + (await _cachedTokenServiceClient.GetAccessToken()).AccessToken;
    }
}
