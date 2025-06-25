using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WTW.Web;
using WTW.Web.Authorization;

namespace WTW.MdpService.Infrastructure.MdpApi;

public class MdpClient : IMdpClient
{
    private readonly HttpClient _client;
    private readonly ILogger<MdpClient> _logger;
    private readonly IHttpContextAccessor _httpContext;

    public MdpClient(HttpClient client, ILogger<MdpClient> logger, IHttpContextAccessor httpContext)
    {
        _client = client;
        _logger = logger;
        _httpContext = httpContext;
    }

    public async Task<ExpandoObject> GetData(IEnumerable<Uri> uris, (string AccessToken, string Env, string Bgroup) auth)
    {
        var flattenedObject = new ExpandoObject();
        (_, string referenceNumber, string businessGroup) = _httpContext.HttpContext.User.User();

        foreach (var url in uris)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("Authorization", auth.AccessToken);
            request.Headers.Add("env", auth.Env);
            request.Headers.Add("Bgroup", auth.Bgroup);
            request.Headers.Add(MdpConstants.BusinessGroupHeaderName, businessGroup); //for single auth
            request.Headers.Add(MdpConstants.ReferenceNumberHeaderName, referenceNumber); //for single auth

            var response = await _client.SendAsync(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var message = $"Failed to retrieve data from url: {url.AbsoluteUri}. Response status code: {response.StatusCode} with reason {response.ReasonPhrase}";
                _logger.LogWarning(message);
                throw new InvalidOperationException(message);
            }

            var data = await response.Content.ReadFromJsonAsync<ExpandoObject>();
            foreach (var prop in data)
            {
                flattenedObject.TryAdd(prop.Key, prop.Value);
            }
            _logger.LogInformation("Successfully retrieved data from url: {dataUrl}.", url.AbsoluteUri);
        }

        return flattenedObject;
    }
}