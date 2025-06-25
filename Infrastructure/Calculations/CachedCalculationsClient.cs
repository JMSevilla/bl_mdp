using System;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using WTW.Web.Caching;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Infrastructure.Calculations;

public class CachedCalculationsClient : ICalculationsClient
{
    private readonly ICalculationsClient _client;
    private readonly ICache _cache;
    private readonly int _expiresInMs;
    private readonly ILogger<CachedCalculationsClient> _logger;

    public CachedCalculationsClient(ICalculationsClient client, ICache cache, int expiresInMs, ILogger<CachedCalculationsClient> logger)
    {
        _client = client;
        _cache = cache;
        _expiresInMs = expiresInMs;
        _logger = logger;
    }

    public TryAsync<RetirementDatesAgesResponse> RetirementDatesAges(string referenceNumber, string businessGroup)
    {
        return async () =>
        {
            var key = $"calc-api-{referenceNumber}-{businessGroup}-retirement-dates-ages";

            return await _cache.Get<RetirementDatesAgesResponse>(key).ToAsync().IfNoneAsync(async () =>
            {
                var datesAges = (await _client.RetirementDatesAges(referenceNumber, businessGroup).Try()).Value();
                await _cache.Set<RetirementDatesAgesResponse>(key, datesAges, TimeSpan.FromMilliseconds(_expiresInMs));
                return datesAges;
            });
        };
    }

    public async Task<Either<Error, (RetirementResponse RetirementResponse, string EventType)>> RetirementCalculation(
        string referenceNumber,
        string businessGroup,
        DateTime effectiveDate)
    {
        var key = $"calc-api-{referenceNumber}-{businessGroup}-retirement-{effectiveDate:yyyy-MM-dd}";

        var cachedRetirement = await _cache.Get<RetirementRedisStore>(key);
        if (cachedRetirement.IsSome)
            return (cachedRetirement.Value().RetirementResponse, cachedRetirement.Value().EventType);

        var retirement = await _client.RetirementCalculation(referenceNumber, businessGroup, effectiveDate);
        if (retirement.IsLeft)
            return retirement.Left();

        var tempStore = new RetirementRedisStore { RetirementResponse = retirement.Right().RetirementResponse, EventType = retirement.Right().EventType };

        await _cache.Set(key, tempStore, TimeSpan.FromMilliseconds(_expiresInMs));
        return retirement.Right();
    }

    public async Task<Either<Error, (RetirementResponse RetirementResponse, string EventType)>> RetirementCalculation(
        string referenceNumber,
        string businessGroup,
        DateTime effectiveDate,
        decimal requestedLumpSum)
    {
        var key = $"calc-api-{referenceNumber}-{businessGroup}-retirement-{effectiveDate:yyyy-MM-dd}-{requestedLumpSum}";

        var cachedRetirement = await _cache.Get<RetirementRedisStore>(key);
        if (cachedRetirement.IsSome && cachedRetirement.Value().RetirementResponse != null)
            return (cachedRetirement.Value().RetirementResponse, cachedRetirement.Value().EventType);

        var retirement = await _client.RetirementCalculation(referenceNumber, businessGroup, effectiveDate, requestedLumpSum);
        if (retirement.IsLeft)
            return retirement.Left();

        var tempStore = new RetirementRedisStore { RetirementResponse = retirement.Right().RetirementResponse, EventType = retirement.Right().EventType };
        await _cache.Set(key, tempStore, TimeSpan.FromMilliseconds(_expiresInMs));
        return retirement.Right();
    }

    public async Task<Either<Error, RetirementResponseV2>> RetirementCalculationV2WithLock(string referenceNumber, string businessGroup, string eventType, DateTime effectiveDate, decimal? requestedLumpSum)
    {
        return await _client.RetirementCalculationV2WithLock(referenceNumber, businessGroup, eventType, effectiveDate, requestedLumpSum);
    }

    public TryAsync<TypeResponse> TransferEventType(string businessGroup, string referenceNumber, string requestType = "transfer")
    {
        return async () =>
        {
            var key = $"calc-api-{referenceNumber}-{businessGroup}-transfer-event-type-{requestType}";

            return await _cache.Get<TypeResponse>(key).ToAsync().IfNoneAsync(async () =>
            {
                var type = (await _client.TransferEventType(businessGroup, referenceNumber, requestType).Try()).Value();
                await _cache.Set<TypeResponse>(key, type, TimeSpan.FromMilliseconds(_expiresInMs));
                return type;
            });
        };
    }

    public async Task<Either<Error, string>> HardQuote(string businessGroup, string referenceNumber)
    {
        return await _client.HardQuote(businessGroup, referenceNumber);
    }

    public async Task<Either<Error, string>> GetGuaranteedTransfer(string businessGroup, string referenceNumber)
    {
        return await _client.GetGuaranteedTransfer(businessGroup, referenceNumber);
    }

    public async Task<Either<Error, (string, int)>> RetirementQuote(string businessGroup, string referenceNumber, DateTime effectiveDate)
    {
        return await _client.RetirementQuote(businessGroup, referenceNumber, effectiveDate);
    }

    public async Task<Either<Error, PartialTransferResponse.MdpResponse>> PartialTransferValues(
        string businessGroup,
        string referenceNumber,
        decimal? requestedTransferValue,
        decimal? requestedResidualPension)
    {
        var key = $"calc-api-{referenceNumber}-{businessGroup}-{requestedTransferValue}-{requestedResidualPension}";

        var cachedRetirement = await _cache.Get<PartialTransferResponse.MdpResponse>(key);
        if (cachedRetirement.IsSome)
            return cachedRetirement.Value();

        var retirement = await _client.PartialTransferValues(businessGroup, referenceNumber, requestedTransferValue, requestedResidualPension);
        if (retirement.IsLeft)
            return retirement.Left();

        await _cache.Set(key, retirement.Right(), TimeSpan.FromMilliseconds(_expiresInMs));
        return retirement.Right();
    }

    public async Task<Either<Error, PartialTransferResponse.MdpResponse>> PensionTranches(
        string businessGroup, string referenceNumber, decimal requestedTransferValue)
    {
        var key = $"calc-api-{referenceNumber}-{businessGroup}-{requestedTransferValue}";

        var cachedRetirement = await _cache.Get<PartialTransferResponse.MdpResponse>(key);
        if (cachedRetirement.IsSome)
            return cachedRetirement.Value();

        var retirement = await _client.PensionTranches(businessGroup, referenceNumber, requestedTransferValue);
        if (retirement.IsLeft)
            return retirement.Left();

        await _cache.Set(key, retirement.Right(), TimeSpan.FromMilliseconds(_expiresInMs));
        return retirement.Right();
    }

    public async Task<Either<Error, TransferResponse>> TransferCalculation(
        string businessGroup, string referenceNumber, bool lockQuote = false)
    {
        return await _client.TransferCalculation(businessGroup, referenceNumber);

        //var key = $"calc-api-{referenceNumber}-{businessGroup}-transfer-calculation";

        //var cachedRetirement = await _cache.Get<TransferResponse>(key);
        //if (cachedRetirement.IsSome)
        //    return cachedRetirement.Value();

        //var retirement = await _client.TransferCalculation(businessGroup, referenceNumber);
        //if (retirement.IsLeft)
        //    return retirement.Left();

        //await _cache.Set(key, retirement.Right(), TimeSpan.FromMilliseconds(_expiresInMs));
        //return retirement.Right();
    }

    public async Task<Either<Error, PartialTransferResponse.MdpResponse>> TransferValues(
        string businessGroup, string referenceNumber, decimal requestedResidualPension)
    {
        var key = $"calc-api-{referenceNumber}-{businessGroup}-{requestedResidualPension}";

        var cachedRetirement = await _cache.Get<PartialTransferResponse.MdpResponse>(key);
        if (cachedRetirement.IsSome)
            return cachedRetirement.Value();

        var retirement = await _client.TransferValues(businessGroup, referenceNumber, requestedResidualPension);
        if (retirement.IsLeft)
            return retirement.Left();

        await _cache.Set(key, retirement.Right(), TimeSpan.FromMilliseconds(_expiresInMs));
        return retirement.Right();
    }

    public async Task<Either<Error, (RetirementResponseV2 RetirementResponseV2, string EventType)>> RetirementCalculationV2(
        string referenceNumber,
        string businessGroup,
        DateTime effectiveDate,
        bool guaranteedQuote,
        bool bypassCache)
    {
        var key = $"calc-api-{referenceNumber}-{businessGroup}-retirementV2-{effectiveDate:yyyy-MM-dd}";

        if (!bypassCache)
        {
            var cachedRetirement = await _cache.Get<RetirementRedisStoreV2>(key);
            if (cachedRetirement.IsSome)
                return (cachedRetirement.Value().RetirementResponseV2, cachedRetirement.Value().EventType);
        }

        var retirement = await _client.RetirementCalculationV2(referenceNumber, businessGroup, effectiveDate, guaranteedQuote);
        if (retirement.IsLeft)
            return retirement.Left();

        var tempStore = new RetirementRedisStoreV2 { RetirementResponseV2 = retirement.Right().RetirementResponseV2, EventType = retirement.Right().EventType };

        await _cache.Set(key, tempStore, TimeSpan.FromMilliseconds(_expiresInMs));
        return retirement.Right();
    }

    public async Task<Either<Error, (RetirementResponseV2 RetirementResponseV2, string EventType)>> RetirementCalculationV2(
        string referenceNumber,
        string businessGroup,
        DateTime effectiveDate,
        decimal requestedLumpSum,
        DateTime? factorDate = null)
    {
        var key = $"calc-api-{referenceNumber}-{businessGroup}-retirementV2-{effectiveDate:yyyy-MM-dd}-{requestedLumpSum}";

        var cachedRetirement = await _cache.Get<RetirementRedisStoreV2>(key);
        if (cachedRetirement.IsSome && cachedRetirement.Value().RetirementResponseV2 != null)
            return (cachedRetirement.Value().RetirementResponseV2, cachedRetirement.Value().EventType);

        var retirement = await _client.RetirementCalculationV2(referenceNumber, businessGroup, effectiveDate, requestedLumpSum, factorDate);
        if (retirement.IsLeft)
            return retirement.Left();

        var tempStore = new RetirementRedisStoreV2 { RetirementResponseV2 = retirement.Right().RetirementResponseV2, EventType = retirement.Right().EventType };
        await _cache.Set(key, tempStore, TimeSpan.FromMilliseconds(_expiresInMs));
        return retirement.Right();
    }

    public async Task<Either<Error, RetirementResponseV2>> RateOfReturn(string businessGroup, string referenceNumber, DateTimeOffset startDate, DateTimeOffset endDate)
    {
        var key = $"calc-api-{referenceNumber}-{businessGroup}-rate-of-return-{startDate:yyyy-MM-dd}-{endDate:yyyy-MM-dd}";

        try
        {
            var cachedRateOfReturn = await _cache.Get<RetirementResponseV2>(key);
            if (cachedRateOfReturn.IsSome)
                return cachedRateOfReturn.Value();

            var rateOfReturn = await _client.RateOfReturn(businessGroup, referenceNumber, startDate, endDate);
            if (rateOfReturn.IsLeft)
                return rateOfReturn.Left();

            await _cache.Set(key, rateOfReturn.Right(), TimeSpan.FromMilliseconds(_expiresInMs));
            return rateOfReturn.Right();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return Error.New(ex.Message);
        }
    }

    public async Task<Either<Error, GetGuaranteedQuoteResponse>> GetGuaranteedQuotes(GetGuaranteedQuoteClientRequest request)
    {
        return await _client.GetGuaranteedQuotes(request);
    }

}