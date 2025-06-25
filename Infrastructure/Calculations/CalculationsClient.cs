using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WTW.Web.Clients;
using WTW.Web.Extensions;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Infrastructure.Calculations;

public class CalculationsClient : ICalculationsClient
{
    private const string BusinessGroupHeaderName = "BGROUP";
    private readonly CalcApiHttpClient _calcApiHttpClient;
    private readonly ILogger<CalculationsClient> _logger;
    private readonly CalculationServiceOptions _options;

    public CalculationsClient(HttpClient client, HttpClient transferCalculationClient, IConfiguration configuration, IHostEnvironment hostEnvironment, ILogger<CalculationsClient> logger, IOptionsSnapshot<CalculationServiceOptions> options)
    {
        _calcApiHttpClient = new CalcApiHttpClient(client, transferCalculationClient, configuration, hostEnvironment);
        _logger = logger;
        _options = options.Value;
    }

    public TryAsync<RetirementDatesAgesResponse> RetirementDatesAges(string referenceNumber, string businessGroup)
    {
        return async () =>
        {
            return await _calcApiHttpClient.Client(businessGroup).GetJson<RetirementDatesAgesResponse>(
                $"bgroups/{businessGroup}/members/{referenceNumber}",
                (BusinessGroupHeaderName, businessGroup));
        };
    }

    public async Task<Either<Error, (RetirementResponse RetirementResponse, string EventType)>> RetirementCalculation(
        string referenceNumber,
        string businessGroup,
        DateTime effectiveDate)
    {
        try
        {

            var typeResponse = (await RetirementEventType(businessGroup, referenceNumber, effectiveDate).Try()).Value();
            var response = await _calcApiHttpClient.Client(businessGroup).GetJson<RetirementResponse>(
                $"bgroups/{businessGroup}/members/{referenceNumber}/calculations/{typeResponse.Type}?&disableTransferCalc=True&effectiveDate={effectiveDate:yyyy-MM-dd}",
                (BusinessGroupHeaderName, businessGroup));

            return response.Errors.Fatals.Any() ? Error.New(string.Join(", ", response.Errors.Fatals)) : (response, typeResponse.Type);
        }
        catch (Exception e)
        {
            return Error.New(e.Message);
        }
    }

    public async Task<Either<Error, (RetirementResponse RetirementResponse, string EventType)>> RetirementCalculation(
        string referenceNumber,
        string businessGroup,
        DateTime effectiveDate,
        decimal requestedLumpSum)
    {
        try
        {
            var typeResponse = (await RetirementEventType(businessGroup, referenceNumber, effectiveDate).Try()).Value();
            var response = await _calcApiHttpClient.Client(businessGroup).GetJson<RetirementResponse>(
                $"bgroups/{businessGroup}/members/{referenceNumber}/calculations/{typeResponse.Type}?" +
                $"disableTransferCalc=True&effectiveDate={effectiveDate:yyyy-MM-dd}&requestedLumpSum={requestedLumpSum}",
                (BusinessGroupHeaderName, businessGroup));

            return response.Errors.Fatals.Any() ? Error.New(string.Join(", ", response.Errors.Fatals)) : (response, typeResponse.Type);
        }
        catch (Exception e)
        {
            return Error.New(e.Message);
        }
    }

    public async Task<Either<Error, RetirementResponseV2>> RetirementCalculationV2WithLock(
        string referenceNumber,
        string businessGroup,
        string eventType,
        DateTime effectiveDate,
        decimal? requestedLumpSum)
    {
        try
        {
            var url = $"bgroups/{businessGroup}/members/{referenceNumber}/calculations/{eventType}?" +
               $"disableTransferCalc=True&effectiveDate={effectiveDate:yyyy-MM-dd}&engineWriteResults=True";
            url = url + (requestedLumpSum.HasValue ? $"&requestedLumpSum={requestedLumpSum}" : string.Empty);
            var response = await _calcApiHttpClient.Client(businessGroup).GetJson<RetirementResponseV2>(url,
                   (BusinessGroupHeaderName, businessGroup));

            return response.Errors.Fatals.Any() ? Error.New(string.Join(", ", response.Errors.Fatals)) : response;
        }
        catch (Exception e)
        {
            return Error.New(e.Message);
        }
    }

    public async Task<Either<Error, string>> HardQuote(string businessGroup, string referenceNumber)
    {
        try
        {
            var typeResponse = (await TransferEventType(businessGroup, referenceNumber).Try()).Value();
            var lockedQuoteResponse = await TransferCalculation(businessGroup, referenceNumber, true);

            if (!lockedQuoteResponse.IsLeft)
            {
                var transferPackResponse = await _calcApiHttpClient.TransferClient(businessGroup).GetJson<TransferPackResponse>(
                    $"bgroups/{businessGroup}/members/{referenceNumber}/calculations/{typeResponse.Type}/letters/default",
                    (BusinessGroupHeaderName, businessGroup));
                return transferPackResponse.LetterURI;
            }

            return lockedQuoteResponse.Left();
        }
        catch (Exception e)
        {
            return Error.New(e.Message);
        }
    }

    public async Task<Either<Error, string>> GetGuaranteedTransfer(string businessGroup, string referenceNumber)
    {
        try
        {
            var typeResponse = (await TransferEventType(businessGroup, referenceNumber).Try()).Value();
            var retirementDateAgesResponse = (await RetirementDatesAges(referenceNumber, businessGroup).Try()).Value();

            if (!retirementDateAgesResponse.HasLockedInTransferQuote)
            {
                var transferCalculationResponse = await _calcApiHttpClient.TransferClient(businessGroup).GetJson<TransferResponse>(
                $"bgroups/{businessGroup}/members/{referenceNumber}/calculations/{typeResponse.Type}?engineWriteResults=true",
                (BusinessGroupHeaderName, businessGroup));

                if (transferCalculationResponse.Errors.Fatals.Any())
                {
                    return Error.New(string.Join(", ", transferCalculationResponse.Errors.Fatals));
                }
            }

            var transferPackResponse = await _calcApiHttpClient.TransferClient(businessGroup).GetJson<TransferPackResponse>(
                    $"bgroups/{businessGroup}/members/{referenceNumber}/calculations/{typeResponse.Type}/letters/default",
                    (BusinessGroupHeaderName, businessGroup));

            return transferPackResponse.LetterURI;
        }
        catch (Exception e)
        {
            return Error.New(e.Message);
        }
    }

    public async Task<Either<Error, GetGuaranteedQuoteResponse>> GetGuaranteedQuotes(GetGuaranteedQuoteClientRequest request)
    {
        try
        {
            var quotationStatus = "";
            if (!request.QuotationStatus.IsNullOrEmpty())
            {
                quotationStatus = $"&status={request.QuotationStatus}";
            }
            var quoteUrl = string.Format(_options.GetGuaranteedQuotesApiPath, request.Bgroup, request.RefNo, request.GuaranteeDateFrom, request.GuaranteeDateTo, request.Event, request.PageNumber, request.PageSize, quotationStatus);

            var getGuaranteedQuotesResponse = await _calcApiHttpClient.Client(request.Bgroup).GetJson<GetGuaranteedQuoteResponse>(quoteUrl, (BusinessGroupHeaderName, request.Bgroup));

            return getGuaranteedQuotesResponse;
        }
        catch (Exception e)
        {
            _logger.LogError($"Exception occured while executing GetGuaranteedQuotes with error message: {e.Message}");

            return Error.New(e.Message);
        }
    }

    public async Task<Either<Error, (string, int)>> RetirementQuote(string businessGroup, string referenceNumber, DateTime effectiveDate)
    {
        try
        {
            var typeResponse = (await RetirementEventType(businessGroup, referenceNumber, effectiveDate).Try()).Value();
            var retirementCalculationResponse = await RetirementCalculationV2WithLock(referenceNumber, businessGroup, typeResponse.Type, effectiveDate, null);

            if (retirementCalculationResponse.IsLeft)
                return retirementCalculationResponse.Left();

            var calcSystemHistorySeqno = retirementCalculationResponse.Right().Results.Mdp.CalcSystemHistorySeqno;

            var retirementPackResponse = await _calcApiHttpClient.Client(businessGroup).GetJson<TransferPackResponse>(
                $"bgroups/{businessGroup}/members/{referenceNumber}/calculations/{typeResponse.Type}/letters/default",
                (BusinessGroupHeaderName, businessGroup));
            return (retirementPackResponse.LetterURI, calcSystemHistorySeqno);
        }
        catch (Exception e)
        {
            return Error.New(e.Message);
        }
    }

    public async Task<Either<Error, TransferResponse>> TransferCalculation(string businessGroup, string referenceNumber, bool lockQuote = false)
    {
        try
        {
            var typeResponse = (await TransferEventType(businessGroup, referenceNumber).Try()).Value();
            var response = await _calcApiHttpClient.TransferClient(businessGroup).GetJson<TransferResponse>(
                $"bgroups/{businessGroup}/members/{referenceNumber}/calculations/{typeResponse.Type}?engineWriteResults={lockQuote}",
                (BusinessGroupHeaderName, businessGroup));

            return response.Errors.Fatals.Any() ? Error.New(string.Join(", ", response.Errors.Fatals)) : response;
        }
        catch (Exception e)
        {
            return Error.New(e.Message);
        }
    }

    public async Task<Either<Error, PartialTransferResponse.MdpResponse>> PensionTranches(
        string businessGroup, string referenceNumber, decimal requestedTransferValue)
    {
        var query = new { requestedTransferValue }.ToQueryString();

        try
        {
            var typeResponse = (await TransferEventType(businessGroup, referenceNumber, "partialTransfer").Try()).Value();
            var response =
                await _calcApiHttpClient.Client(businessGroup).GetJson<PartialTransferResponse>(
                    $"bgroups/{businessGroup}/members/{referenceNumber}/calculations/{typeResponse.Type}?{query}",
                    (BusinessGroupHeaderName, businessGroup));

            return response.Errors.Fatals.Any() ? Error.New(string.Join(", ", response.Errors.Fatals)) : response.Results.Mdp;
        }
        catch (Exception e)
        {
            return Error.New(e.Message);
        }
    }

    public async Task<Either<Error, PartialTransferResponse.MdpResponse>> TransferValues(
        string businessGroup, string referenceNumber, decimal requestedResidualPension)
    {

        var query = new { requestedResidualPension }.ToQueryString();

        try
        {
            var typeResponse = (await TransferEventType(businessGroup, referenceNumber, "partialTransfer").Try()).Value();
            var response =
                await _calcApiHttpClient.Client(businessGroup).GetJson<PartialTransferResponse>(
                    $"bgroups/{businessGroup}/members/{referenceNumber}/calculations/{typeResponse.Type}?{query}",
                    (BusinessGroupHeaderName, businessGroup));

            return response.Errors.Fatals.Any() ? Error.New(string.Join(", ", response.Errors.Fatals)) : response.Results.Mdp;
        }
        catch (Exception e)
        {
            return Error.New(e.Message);
        }
    }

    public async Task<Either<Error, PartialTransferResponse.MdpResponse>> PartialTransferValues(
        string businessGroup,
        string referenceNumber,
        decimal? requestedTransferValue,
        decimal? requestedResidualPension)
    {
        var query = new { requestedTransferValue, requestedResidualPension }.ToQueryString();

        try
        {
            var typeResponse = (await TransferEventType(businessGroup, referenceNumber, "partialTransfer").Try()).Value();
            var response =
                await _calcApiHttpClient.Client(businessGroup).GetJson<PartialTransferResponse>(
                    $"bgroups/{businessGroup}/members/{referenceNumber}/calculations/{typeResponse.Type}?{query}",
                    (BusinessGroupHeaderName, businessGroup));

            return response.Errors.Fatals.Any() ? Error.New(string.Join(", ", response.Errors.Fatals)) : response.Results.Mdp;
        }
        catch (Exception e)
        {
            return Error.New(e.Message);
        }
    }

    public TryAsync<TypeResponse> TransferEventType(string businessGroup, string referenceNumber, string requestType = "transfer")
    {
        return async () =>
        {
            return await _calcApiHttpClient.Client(businessGroup).GetJson<TypeResponse>(
            $"bgroups/{businessGroup}/members/{referenceNumber}/events/{requestType}",
            (BusinessGroupHeaderName, businessGroup));
        };
    }

    public async Task<Either<Error, (RetirementResponseV2 RetirementResponseV2, string EventType)>> RetirementCalculationV2(
        string referenceNumber,
        string businessGroup,
        DateTime effectiveDate,
        bool engineGuaranteeQuote,
        bool bypassCache = false)
    {
        try
        {
            var typeResponse = (await RetirementEventType(businessGroup, referenceNumber, effectiveDate).Try()).Value();

            var calcUrl = $"bgroups/{businessGroup}/members/{referenceNumber}/calculations/{typeResponse.Type}?&disableTransferCalc=True&effectiveDate={effectiveDate:yyyy-MM-dd}";

            if (_options.GuaranteedQuotesEnabledFor.Contains(businessGroup))
            {
                calcUrl = $"bgroups/{businessGroup}/members/{referenceNumber}/calculations/{typeResponse.Type}?&disableTransferCalc=True&effectiveDate={effectiveDate:yyyy-MM-dd}&engineGuaranteeQuote={engineGuaranteeQuote}&engineWriteResults=true";
            }

            var response = await _calcApiHttpClient.Client(businessGroup).GetJson<RetirementResponseV2>(calcUrl,
                (BusinessGroupHeaderName, businessGroup));

            return response.Errors.Fatals.Any() ? Error.New(string.Join(", ", response.Errors.Fatals), "noFigures") : (response, typeResponse.Type);
        }
        catch (Exception e)
        {
            return Error.New(e.Message);
        }
    }

    public async Task<Either<Error, (RetirementResponseV2 RetirementResponseV2, string EventType)>> RetirementCalculationV2(
        string referenceNumber,
        string businessGroup,
        DateTime effectiveDate,
        decimal requestedLumpSum,
        DateTime? factorDate = null)
    {
        try
        {
            var typeResponse = (await RetirementEventType(businessGroup, referenceNumber, effectiveDate).Try()).Value();

            var calcUrl = $"bgroups/{businessGroup}/members/{referenceNumber}/calculations/{typeResponse.Type}?" +
                $"disableTransferCalc=True&effectiveDate={effectiveDate:yyyy-MM-dd}&requestedLumpSum={requestedLumpSum}";

            if (_options.GuaranteedQuotesEnabledFor.Contains(businessGroup))
            {
                calcUrl = $"{calcUrl}&factorDate={factorDate?.ToString("yyyy-MM-dd")}";
            }
            var response = await _calcApiHttpClient.Client(businessGroup).GetJson<RetirementResponseV2>(calcUrl,
                (BusinessGroupHeaderName, businessGroup));

            return response.Errors.Fatals.Any() ? Error.New(string.Join(", ", response.Errors.Fatals)) : (response, typeResponse.Type);
        }
        catch (Exception e)
        {
            return Error.New(e.Message);
        }
    }

    private TryAsync<TypeResponse> RetirementEventType(string businessGroup, string referenceNumber, DateTime effectiveDate)
    {
        return async () =>
        {
            return await _calcApiHttpClient.Client(businessGroup).GetJson<TypeResponse>(
                $"bgroups/{businessGroup}/members/{referenceNumber}/events/retirement?effectiveDate={effectiveDate:yyyy-MM-dd}",
                (BusinessGroupHeaderName, businessGroup));
        };
    }

    public async Task<Either<Error, RetirementResponseV2>> RateOfReturn(
        string businessGroup,
        string referenceNumber,
        DateTimeOffset startDate,
        DateTimeOffset effectiveDate)
    {
        try
        {
            var url = $"bgroups/{businessGroup}/members/{referenceNumber}/calculations/RR?" +
                      $"startDate={startDate:yyyy-MM-dd}&effectiveDate={effectiveDate:yyyy-MM-dd}";
            var response = await _calcApiHttpClient.Client(businessGroup).GetJson<RetirementResponseV2>(url,
                   (BusinessGroupHeaderName, businessGroup));

            return response.Errors.Fatals.Any() ? Error.New(string.Join(", ", response.Errors.Fatals)) : response;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return Error.New(e.Message);
        }
    }
}