using System;
using System.Net.Http;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace WTW.MdpService.Infrastructure.RetryPolicy;

public static class GenericApiPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> RetryPolicy(RetryPolicyOptions config)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                config.GeneralRetryCount,
                retryAttempt => TimeSpan.FromSeconds(config.GeneralRetryDelay),
                (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} encountered an error: {exception?.Exception.Message}. Waiting {timeSpan} before next retry.");
                });
    }

    public static IAsyncPolicy<HttpResponseMessage> TimeoutPolicy(TimeSpan timeSpan)
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(timeSpan);
    }
}