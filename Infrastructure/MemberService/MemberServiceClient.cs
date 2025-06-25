using System.Net.Http;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WTW.MdpService.Beneficiaries;
using WTW.MdpService.Infrastructure.TokenService;
using WTW.Web;
using WTW.Web.Clients;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Infrastructure.MemberService;

public class MemberServiceClient : IMemberServiceClient
{

    private readonly HttpClient _client;
    private readonly ICachedTokenServiceClient _cachedTokenServiceClient;
    private readonly MemberServiceOptions _options;
    private readonly ILogger<MemberServiceClient> _logger;

    public MemberServiceClient(HttpClient client,
                               ICachedTokenServiceClient cachedTokenServiceClient,
                               IOptionsSnapshot<MemberServiceOptions> options,
                               ILogger<MemberServiceClient> logger)
    {
        _client = client;
        _cachedTokenServiceClient = cachedTokenServiceClient;
        _options = options.Value;
        _logger = logger;
    }


    public async Task<Option<BeneficiariesV2Response>> GetBeneficiaries(string businessGroup,
                                                                       string referenceNumber,
                                                                       bool includeRevoked,
                                                                       bool refreshCache)
    {
        var result = await _client.GetOptionalJson<BeneficiariesV2Response>(
          string.Format(_options.GetBeneficiariesPath, businessGroup, referenceNumber, includeRevoked, refreshCache),
           (MdpConstants.AuthorizationHeaderName, $"{await GetAccessToken()}"));
        if (result.IsNone)
        {
            _logger.LogWarning("GetBeneficiaries details not found for refno {refno}", referenceNumber);
            return result;
        }

        return result.Value();
    }

    public async Task<Option<GetLinkedRecordClientResponse>> GetBcUkLinkedRecord(string bgroup, string refNo)
    {
        var token = await GetAccessToken();

        var result = await _client.GetOptionalJson<GetLinkedRecordClientResponse>(
               string.Format(_options.GetLinkedMemberPath, bgroup, refNo),
               (MdpConstants.AuthorizationHeaderName, $"{token}"));

        if (result.IsNone)
        {
            _logger.LogWarning("Linked record not found for refno {refno}", refNo);
            return Option<GetLinkedRecordClientResponse>.None;
        }

        return result.Value();
    }

    public async Task<Option<GetPensionDetailsClientResponse>> GetPensionDetails(string bgroup, string refNo)
    {
        var token = await GetAccessToken();

        var result = await _client.GetOptionalJson<GetPensionDetailsClientResponse>(
           string.Format(_options.GetPensionDetailsPath, bgroup, refNo),
           (MdpConstants.AuthorizationHeaderName, $"{token}"));

        if (result.IsNone)
        {
            _logger.LogWarning("Member pension details not found for {refno}", refNo);
            return Option<GetPensionDetailsClientResponse>.None;
        }

        return result.Value();
    }

    public async Task<Option<GetMemberSummaryClientResponse>> GetMemberSummary(string bgroup, string refNo)
    {
        var token = await GetAccessToken();

        var result = await _client.GetOptionalJson<GetMemberSummaryClientResponse>(
         string.Format(_options.GetMemberSummaryPath, bgroup, refNo),
         (MdpConstants.AuthorizationHeaderName, $"{token}"));

        if (result.IsNone)
        {
            _logger.LogWarning("Member summary details not found for {refno}", refNo);
            return Option<GetMemberSummaryClientResponse>.None;
        }

        return result.Value();
    }

    public async Task<Option<GetMemberPersonalDetailClientResponse>> GetPersonalDetail(string bgroup, string refNo)
    {
        var token = await GetAccessToken();

        var result = await _client.GetOptionalJson<GetMemberPersonalDetailClientResponse>(
          string.Format(_options.GetPersonalDetailPath, bgroup, refNo),
          (MdpConstants.AuthorizationHeaderName, $"{token}"));

        if (result.IsNone)
        {
            _logger.LogWarning("Member personal details not found for {refno}", refNo);
            return Option<GetMemberPersonalDetailClientResponse>.None;
        }

        return result.Value();
    }

    public async Task<Option<MemberContactDetailsClientResponse>> GetContactDetails(string bgroup, string refNo)
    {
        var token = await GetAccessToken();

        var result = await _client.GetOptionalJson<MemberContactDetailsClientResponse>(
          string.Format(_options.GetContactDetailsPath, bgroup, refNo),
          (MdpConstants.AuthorizationHeaderName, $"{token}"));

        if (result.IsNone)
        {
            _logger.LogWarning("Member contact details not found for {refno}", refNo);
            return Option<MemberContactDetailsClientResponse>.None;
        }

        return result.Value();
    }

    public async Task<Option<GetMatchingMemberClientResponse>> GetMemberMatchingRecords(string bgroup, GetMemberMatchingClientRequest request)
    {
        var token = await GetAccessToken();
        try
        {
            return await _client.PostJson<GetMemberMatchingClientRequest, GetMatchingMemberClientResponse>(
         string.Format(_options.GetMemberMatchingAbsolutePath, bgroup),
          request, (MdpConstants.AuthorizationHeaderName, $"{token}"));
        }
        catch (HttpRequestException ex) when (ex.StatusCode.Equals(System.Net.HttpStatusCode.NotFound))
        {
            _logger.LogWarning("No matching record found");
            return Option<GetMatchingMemberClientResponse>.None;
        }
    }


    private async Task<string> GetAccessToken()
    {
        return "Bearer " + (await _cachedTokenServiceClient.GetAccessToken()).AccessToken;
    }
}
