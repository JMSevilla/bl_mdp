using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace WTW.MdpService.Infrastructure.Calculations;

public class CalcApiHttpClient
{
    private readonly HttpClient _client;
    private readonly HttpClient _transferCalculationClient;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _hostEnvironment;
    private const string CALCAPIURL = ".awstas.net/";
    
    public CalcApiHttpClient(HttpClient client, HttpClient transferCalculationClient, IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        _client = client;
        _configuration = configuration;
        _hostEnvironment = hostEnvironment;
        _transferCalculationClient = transferCalculationClient;
    }
    
    public HttpClient Client(string businessGroup)
    {
        SetBaseUrl(_client, businessGroup);
        return _client;
    }
    
    public HttpClient TransferClient(string businessGroup)
    {
        SetBaseUrl(_transferCalculationClient, businessGroup);
        return _transferCalculationClient;
    }
    
    private void SetBaseUrl(HttpClient client, string businessGroup)
    {
        var url = new Uri("https://" + _configuration[$"Tenants:{_hostEnvironment.EnvironmentName}:{businessGroup}"] + CALCAPIURL);
        if(client.BaseAddress != url)
            client.BaseAddress = url;
    }
}