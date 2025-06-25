using System;
using System.Net;
using System.Net.Http;
using Polly;

namespace WTW.MdpService.Infrastructure.RetryPolicy;

public static class CalculationApiPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> RetryPolicy(RetryPolicyOptions config)
    {
        return Policy
            .HandleResult<HttpResponseMessage>(msg => msg.StatusCode == (HttpStatusCode)425)
            .WaitAndRetryAsync(
                config.RetryCountFor425,
                retryAttempt => TimeSpan.FromSeconds(config.RetryDelayFor425),
                (outcome, timespan, retryAttempt, context) =>
                {
                    Console.WriteLine($"Retry {retryAttempt} for Calculation API {outcome.Result.StatusCode}. Waiting {timespan} before next retry.");
                });
    }
}