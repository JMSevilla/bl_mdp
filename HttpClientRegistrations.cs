using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using WTW.MdpService.Infrastructure.BankService;
using WTW.MdpService.Infrastructure.DeloreanAuthentication;
using WTW.MdpService.Infrastructure.EpaService;
using WTW.MdpService.Infrastructure.IdvService;
using WTW.MdpService.Infrastructure.IpaService;
using WTW.MdpService.Infrastructure.MemberService;
using WTW.MdpService.Infrastructure.MemberWebInteractionService;
using WTW.MdpService.Infrastructure.TelephoneNoteService;
using WTW.Web;

namespace WTW.MdpService;

public static class HttpClientRegistrations
{
    public static void AddHttpClients(IServiceCollection services, ConfigurationManager configuration)
    {
        try
        {
            var authenticationClientBaseAddress = new Uri(configuration.GetSection(MdpConstants.ConfigSection.DeloreanAuthenticationBaseUrlName).Value);
            services.AddHttpClient<IDeloreanAuthenticationClient, DeloreanAuthenticationClient>
                (
                    client =>
                    {
                        client.BaseAddress = authenticationClientBaseAddress;
                    }
                )
                .AddPolicyHandler(GetRetryPolicy());

            var memberClientBaseAddress = new Uri(configuration.GetSection(MdpConstants.ConfigSection.MemberServiceBaseUrlName).Value);
            services.AddHttpClient<IMemberServiceClient, MemberServiceClient>
              (
                  client =>
                  {
                      client.BaseAddress = memberClientBaseAddress;
                  }
              )
              .AddPolicyHandler(GetRetryPolicy());

            var memberWebInteractionClientBaseAddress = new Uri(configuration.GetSection(MdpConstants.ConfigSection.MemberWebInteractionServiceBaseUrlName).Value);
            services.AddHttpClient<IMemberWebInteractionServiceClient, MemberWebInteractionServiceClient>
              (
                  client =>
                  {
                      client.BaseAddress = memberWebInteractionClientBaseAddress;
                  }
              )
              .AddPolicyHandler(GetRetryPolicy());

            var epaClientBaseAddress = new Uri(configuration.GetSection(MdpConstants.ConfigSection.EpaServiceBaseUrlName).Value);
            services.AddHttpClient<IEpaServiceClient, EpaServiceClient>
              (
                  client =>
                  {
                      client.BaseAddress = epaClientBaseAddress;
                  }
              )
              .AddPolicyHandler(GetRetryPolicy());

            var bankServiceClientBaseAddress = new Uri(configuration.GetSection(MdpConstants.ConfigSection.BankServiceBaseUrlName).Value);
            services.AddHttpClient<IBankServiceClient, BankServiceClient>
              (
                  client =>
                  {
                      client.BaseAddress = bankServiceClientBaseAddress;
                  }
              )
              .AddPolicyHandler(GetRetryPolicy());

            var idvServiceClientBaseAddress = new Uri(configuration.GetSection(MdpConstants.ConfigSection.IdentityVerificationBaseUrlName).Value);
            services.AddHttpClient<IIdentityVerificationClient, IdentityVerificationClient>
              (
                  client =>
                  {
                      client.BaseAddress = idvServiceClientBaseAddress;
                  }
              )
              .AddPolicyHandler(GetRetryPolicy());

            var ipaServiceClientBaseAddress = new Uri(configuration.GetSection(MdpConstants.ConfigSection.IpaServiceBaseUrlName).Value);
            services.AddHttpClient<IIpaServiceClient, IpaServiceClient>
              (
                  client =>
                  {
                      client.BaseAddress = ipaServiceClientBaseAddress;
                  }
              )
              .AddPolicyHandler(GetRetryPolicy());

            var telephoneNoteServiceClientBaseAddress = new Uri(configuration.GetSection(MdpConstants.ConfigSection.TelephoneNoteServiceBaseUrlName).Value);
            services.AddHttpClient<ITelephoneNoteServiceClient, TelephoneNoteServiceClient>
              (
                  client =>
                  {
                      client.BaseAddress = telephoneNoteServiceClientBaseAddress;
                  }
              )
              .AddPolicyHandler(GetRetryPolicy());
        }
        catch (ArgumentNullException)
        {
            Console.WriteLine($"{MdpConstants.ConfigSection.DeloreanAuthenticationBaseUrlName} {MdpConstants.ConfigSection.NotConfiguredErrorMessage}");
            throw;
        }
    }
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int maxRetry = 6, int initialSleepDurationMilliseconds = 200)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(maxRetry, retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt - 1) * initialSleepDurationMilliseconds),
                onRetry: (result, timespan, retryAttempt, _) =>
                {
                    Console.WriteLine($"{result.Result?.RequestMessage?.RequestUri} Delaying for {timespan} ms, then making retry {retryAttempt}.");
                });
    }
}
