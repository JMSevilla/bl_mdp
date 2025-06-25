using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WTW.Web.Caching;

namespace WTW.MdpService.Infrastructure.Redis;

public class CalculationsRedisCache : ICalculationsRedisCache
{
    private readonly ICache _cache;
    private readonly ILogger<CalculationsRedisCache> _logger;

    public CalculationsRedisCache(ICache cache, ILogger<CalculationsRedisCache> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task Clear(string referenceNumber, string businessGroup)
    {
        try
        {
            await _cache.RemoveByPrefix($"calc-api-{referenceNumber}-{businessGroup}-");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear calculations api cache");
        }
    }
    
    public async Task ClearRetirementDateAges(string referenceNumber, string businessGroup)
    {
        try
        {
            await _cache.RemoveByPrefix($"calc-api-{referenceNumber}-{businessGroup}-retirement-dates-ages");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear calculations api cache");
        }
    }
}