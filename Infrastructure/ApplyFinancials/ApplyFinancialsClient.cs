using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using WTW.Web.Clients;

namespace WTW.MdpService.Infrastructure.ApplyFinancials;

public class ApplyFinancialsClient : IApplyFinancialsClient
{
    private readonly HttpClient _client;
    private readonly string _userName;
    private readonly string _password;

    public ApplyFinancialsClient(HttpClient client, string userName, string password)
    {
        _client = client;
        _userName = userName;
        _password = password;
    }

    public async Task<Either<Error, AccountValidationResponse>> ValidateIbanBankAccount(
        string iban,
        string bic,
        string countryCode)
    {
        var urlQueryParametersForIbanAccountValidation = $"countryCode={countryCode}&token={await GetToken()}&accountNumber={iban}&nationalId={bic}";
        return await ValidateBankAccount(urlQueryParametersForIbanAccountValidation);
    }

    public async Task<Either<Error, AccountValidationResponse>> ValidateUkBankAccount(string accountNumber, string sortCode)
    {
        var urlQueryParametersForUkBankAccountValidation = $"countryCode=GB&token={await GetToken()}&accountNumber={accountNumber}&nationalId={sortCode}";
        return await ValidateBankAccount(urlQueryParametersForUkBankAccountValidation);
    }

    private async Task<Either<Error, AccountValidationResponse>> ValidateBankAccount(string urlQueryParameters)
    {
        AccountValidationResponse result;
        try
        {
            result = await _client.GetJson<AccountValidationResponse>("/validate-api/rest/convert/1.0.1?" + urlQueryParameters);
        }
        catch (HttpRequestException ex)
        {
            if (IsConfigurationException(ex))
                throw;

            return Error.New("access_to_external_bank_details_failed");
        }

        if (result.Status == "FAIL" || result.Status == "CAUTION")
            return Error.New("failed_external_bank_details_validation");

        return result;
    }

    private async Task<string> GetToken()
    {
        var result = await _client.PostFromUrlEncoded<TokenResponse>("/validate-api/rest/authenticate",
            new Dictionary<string, string> { { "username", _userName }, { "password", _password } });

        if (string.IsNullOrWhiteSpace(result.Token))
            throw new InvalidOperationException("Apply Finance API: Username and password do not match.");

        return result.Token;
    }

    private static bool IsConfigurationException(HttpRequestException ex)
    {
        return (ex.StatusCode.HasValue && ((int)ex.StatusCode.Value).ToString().StartsWith("4")) ||
            ex.Message.Contains("No such host is known.");
    }
}