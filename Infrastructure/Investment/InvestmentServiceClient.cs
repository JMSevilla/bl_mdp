using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using WTW.MdpService.DcRetirement;
using WTW.MdpService.Infrastructure.Investment.AnnuityBroker;
using WTW.MdpService.Infrastructure.TokenService;
using WTW.Web;
using WTW.Web.Clients;

namespace WTW.MdpService.Infrastructure.Investment;

public class InvestmentServiceClient : IInvestmentServiceClient
{
    private const string BusinessGroupHeaderName = "BGROUP";
    private const string MappingType = "LS_SPENDING";
    private readonly HttpClient _client;
    private readonly ICachedTokenServiceClient _cachedTokenServiceClient;
    private readonly ILogger<InvestmentServiceClient> _logger;

    public InvestmentServiceClient(HttpClient client, ICachedTokenServiceClient cachedTokenServiceClient, ILogger<InvestmentServiceClient> logger)
    {
        _client = client;
        _cachedTokenServiceClient = cachedTokenServiceClient;
        _logger = logger;
    }

    public async Task<Option<InvestmentInternalBalanceResponse>> GetInternalBalance(string referenceNumber, string businessGroup, string schemeType)
    {
        try
        {
            return (await _client.GetJson<InvestmentInternalBalanceResponse, InvestmentServiceErrorResponse>(
                $"internal/v1/bgroups/{businessGroup}/members/{referenceNumber}/balances",
                (BusinessGroupHeaderName, businessGroup),
                ("Authorization", $"{await GetAccessToken()}")))
                .Match(
                   x => x,
                   error =>
                   {
                       _logger.LogError("Internal balance endpoint for {businessGroup} {referenceNumber} " +
                                        "returned error: Message: {message}. Code: {code}", businessGroup, referenceNumber, error.Message, error.Code);
                       return Option<InvestmentInternalBalanceResponse>.None;
                   }
                );

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve internal balance for {businessGroup} {referenceNumber}.", businessGroup, referenceNumber);
            return Option<InvestmentInternalBalanceResponse>.None;
        }
    }

    public async Task<Either<Error, InvestmentForecastResponse>> GetInvestmentForecast(string referenceNumber, string businessGroup, int targetAge, string schemeType)
    {
        if (schemeType != "DC")
            return new InvestmentForecastResponse();

        try
        {
            return await GetForecastResponse(referenceNumber, businessGroup, targetAge);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve projected balances for {businessGroup} {referenceNumber}.", businessGroup, referenceNumber);
            return new InvestmentForecastResponse();
        }
    }

    public async Task<Option<InvestmentForecastResponse>> GetInvestmentForecast(string referenceNumber, string businessGroup, IEnumerable<int> targetAges)
    {
        try
        {
            return await GetForecastResponse(referenceNumber, businessGroup, targetAges.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve projected balances for {businessGroup} {referenceNumber}.", businessGroup, referenceNumber);
            return Option<InvestmentForecastResponse>.None;
        }
    }

    public async Task<Option<InvestmentForecastAgeResponse>> GetInvestmentForecastAge(string referenceNumber, string businessGroup)
    {
        try
        {
            var uri = $"internal/v1/bgroups/{businessGroup}/members/{referenceNumber}/forecast/age";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("Authorization", $"{await GetAccessToken()}");
            var response = await _client.SendAsync(request);

            _logger.LogInformation("Response from investment service. Uri: {uri}. Response body: {responseBody}.", uri, await response.Content.ReadAsStringAsync());

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.BadRequest)
            {
                var error = await response.Content.ReadFromJsonAsync<InvestmentServiceErrorResponse>();
                _logger.LogError("Investment forecast age and date endpoint for {businessGroup} {referenceNumber}. " +
                    "Returned error: Message: {message}. Code: {code}", businessGroup, referenceNumber, error.Message, error.Code);
                return Option<InvestmentForecastAgeResponse>.None;
            }

            return await response.Content.ReadFromJsonAsync<InvestmentForecastAgeResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve investment forecast age and date for {businessGroup} {referenceNumber}.", businessGroup, referenceNumber);
            return Option<InvestmentForecastAgeResponse>.None;
        }
    }

    public async Task<Option<DcSpendingResponse<StrategyContributionTypeResponse>>> GetInvestmentStrategies(string businessGroup, string schemeCode, string contType)
    {
        try
        {
            return (await _client.GetJson<DcSpendingResponse<StrategyContributionTypeResponse>, InvestmentServiceErrorResponse>(
                 $"internal/v1/bgroups/{businessGroup}/schemes/{schemeCode}/strategies?conttype={contType}",
                (BusinessGroupHeaderName, businessGroup),
                ("Authorization", $"{await GetAccessToken()}")))
                .Match(
                    x => x,
                    error =>
                    {
                        _logger.LogError("Investment strategies endpoint for {businessGroup} returned error: Message: {message}. Code: {code}", businessGroup, error.Message, error.Code);
                        return Option<DcSpendingResponse<StrategyContributionTypeResponse>>.None;
                    });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve investment strategies.");
            return Option<DcSpendingResponse<StrategyContributionTypeResponse>>.None;
        }
    }

    public async Task<Option<DcSpendingResponse<FundContributionTypeResponse>>> GetInvestmentFunds(string businessGroup, string schemeCode, string contType)
    {
        try
        {
            return (await _client.GetJson<DcSpendingResponse<FundContributionTypeResponse>, InvestmentServiceErrorResponse>(
                 $"internal/v1/bgroups/{businessGroup}/schemes/{schemeCode}/funds?conttype={contType}",
                (BusinessGroupHeaderName, businessGroup),
                ("Authorization", $"{await GetAccessToken()}")))
                .Match(
                    x => x,
                    error =>
                    {
                        _logger.LogError("Investment funds endpoint for {businessGroup} returned error: Message: {message}. Code: {code}", businessGroup, error.Message, error.Code);
                        return Option<DcSpendingResponse<FundContributionTypeResponse>>.None;
                    });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve investment funds.");
            return Option<DcSpendingResponse<FundContributionTypeResponse>>.None;
        }
    }

    public async Task<Option<TargetSchemeMappingResponse>> GetTargetSchemeMappings(string businessGroup, string schemeCode, string category)
    {
        try
        {
            return (await _client.GetJson<TargetSchemeMappingResponse, InvestmentServiceErrorResponse>(
                 $"internal/v1/bgroups/{businessGroup}/schemes/{schemeCode}/categories/{category}/target-scheme-mappings?mappingType={MappingType}",
                (BusinessGroupHeaderName, businessGroup),
                ("Authorization", $"{await GetAccessToken()}")))
                .Match(
                    x => x,
                    error =>
                    {
                        _logger.LogError("Target scheme mappings endpoint for {businessGroup} returned error: Message: {message}. Code: {code}", businessGroup, error.Message, error.Code);
                        return Option<TargetSchemeMappingResponse>.None;
                    });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve target scheme mappings.");
            return Option<TargetSchemeMappingResponse>.None;
        }
    }

    public async Task<Option<LatestContributionResponse>> GetLatestContribution(string businessGroup, string referenceNumber)
    {
        try
        {
            return (await _client.GetJson<LatestContributionResponse, InvestmentServiceErrorResponse>(
                 $"internal/v1/bgroups/{businessGroup}/members/{referenceNumber}/latest-contribution",
                (BusinessGroupHeaderName, businessGroup),
                ("Authorization", $"{await GetAccessToken()}")))
                .Match(
                    x => x,
                    error =>
                    {
                        _logger.LogError("Latest Contribution endpoint for {businessGroup}-{referenceNumber} returned error: Message: {message}. Code: {code}", businessGroup, referenceNumber, error.Message, error.Code);
                        return Option<LatestContributionResponse>.None;
                    });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve latest Contribution.");
            return Option<LatestContributionResponse>.None;
        }
    }

    public async Task<Either<Error, Unit>> CreateAnnuityQuote(string businessGroup, string referenceNumber, InvestmentQuoteRequest request)
    {
        var token = await GetAccessToken();

        try
        {
            var uri = $"internal/v1/bgroups/{businessGroup}/members/{referenceNumber}/annuity/quote";
            var response = await _client.PostJson(uri, request, (BusinessGroupHeaderName, businessGroup), (MdpConstants.AuthorizationHeaderName, $"{token}"));
            response.EnsureSuccessStatusCode();

            return Unit.Default;
        }
        catch (Exception ex)
        {
            _logger.LogError("Annuity quote request failed. Error: {error}", ex.Message);
            return Error.New(ex.Message);
        }
    }

    private async Task<InvestmentForecastResponse> GetForecastResponse(string referenceNumber, string businessGroup, params int[] targetAges)
    {
        return await _client.PostJson<InvestmentForecastRequest, InvestmentForecastResponse>($"internal/v1/bgroups/{businessGroup}/members/{referenceNumber}/forecast/investment",
                new InvestmentForecastRequest
                {
                    Ages = targetAges.ToList()
                },
                (BusinessGroupHeaderName, businessGroup),
                ("Authorization", $"{await GetAccessToken()}"));
    }

    private async Task<string> GetAccessToken()
    {
        return "Bearer " + (await _cachedTokenServiceClient.GetAccessToken()).AccessToken;
    }
}