using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Infrastructure.CasesApi;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.TransferJourneys;

public class TransferCase : ITransferCase
{
    private ICasesClient _caseClient;
    private readonly ILogger<TransferCase> _logger;

    public TransferCase(ICasesClient caseClient, ILogger<TransferCase> logger)
    {
        _caseClient = caseClient;
        _logger = logger;
    }

    public async Task<Either<Error, string>> Create(string businessGroup, string  referenceNumber)
    {
        var request = new CreateCaseRequest
        {
            BusinessGroup = businessGroup,
            ReferenceNumber = referenceNumber,
            CaseCode = "TOP9",
            BatchSource = "MDP",
            BatchDescription = "Transfer Out case created by an online application",
            Narrative = "",
            Notes = "Case created by an online transfer application",
            StickyNotes = "Case created by an online transfer application"
        };

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

            return Error.New($"Business group: {response.BusinessGroup}. Case: {response.CaseNumber} was create. But case exist returns false after 30 times every 2 seconds of trying.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to check if created member case exist. Case number: {response.CaseNumber}," +
                $" Batch number: {response.BatchNumber}, Business group: {response.BusinessGroup}");
            throw;
        }
    }
}