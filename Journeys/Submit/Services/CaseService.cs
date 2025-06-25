using System;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Infrastructure.CasesApi;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Journeys.Submit.Services;

public class CaseService : ICaseService
{
    private readonly ICasesClient _caseClient;
    private readonly ILogger<CaseService> _logger;

    public CaseService(ICasesClient caseClient, ILogger<CaseService> logger)
    {
        _caseClient = caseClient;
        _logger = logger;
    }

    public async Task<Either<Error, string>> Create(CreateCaseRequest request)
    {
        Either<CreateCaseError, CreateCaseResponse> errorOrResponse;
        try
        {
            errorOrResponse = await _caseClient.CreateForMember(request);
            if (errorOrResponse.IsLeft)
                return Error.New(errorOrResponse.Left().Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create case for member.");
            throw;
        }

        var response = errorOrResponse.Right();
        try
        {
            var counter = 1;
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(2));

            Either<CreateCaseError, CaseExistsResponse> errorOrResult;
            do
            {
                errorOrResult = await _caseClient.Exists(response.BusinessGroup, response.CaseNumber);
                if (errorOrResult.IsRight && errorOrResult.Right().CaseExists)
                    return response.CaseNumber;

                counter++;
            } while (await timer.WaitForNextTickAsync() && counter <= 90);

            if (errorOrResult.IsLeft)
                return Error.New(errorOrResult.Left().Message);

            return Error.New($"Business group: {response.BusinessGroup}. Case: {response.CaseNumber} was created. But case exist returns false after 30 times every 2 seconds of trying.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if created member case exist. Case number: {caseNumber}," +
                " Batch number: {batchNumber}, Business group: {businessGroup}", response.CaseNumber, response.BatchNumber, response.BusinessGroup);
            throw;
        }
    }
}