using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Infrastructure.CasesApi;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.BereavementJourneys;

public class BereavementCase : IBereavementCase
{
    private readonly ICasesClient _caseClient;
    private readonly ILogger<BereavementCase> _logger;

    public BereavementCase(ICasesClient caseClient, ILogger<BereavementCase> logger)
    {
        _caseClient = caseClient;
        _logger = logger;
    }

    public async Task<Either<Error, string>> Create(string businessGroup, string name, string surname, DateTime? dateOfBirth, DateTime? dateOfDeath, IEnumerable<string> refNumbers)
    {
        var request = new CreateCaseRequest
        {
            BusinessGroup = businessGroup,
            CaseCode = "NDD9",
            BatchSource = "MDP",
            BatchDescription = "Death notification case created by an online application",
            Narrative = "",
            Notes = "Case created by an online bereavement application",
            StickyNotes = $"The deceased member details are: Name: {name}, Surname: {surname}, DOB:" +
                $" {dateOfBirth:dd/MM/yyyy}, Date of Death: {dateOfDeath:dd/MM/yyyy}" + AddReferenceNumbers(refNumbers),
        };

        Either<CreateCaseError, CreateCaseResponse> errorOrResponse;
        try
        {
            errorOrResponse = await _caseClient.CreateForNonMember(request);
            if (errorOrResponse.IsLeft)
                return Error.New(errorOrResponse.Left().Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create case for non member.");
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
            _logger.LogError(ex, $"Failed to check if create bereavement case exist. Case number: {response.CaseNumber}," +
                $" Batch number: {response.BatchNumber}, Business group: {response.BusinessGroup}");
            throw;
        }
    }

    private string AddReferenceNumbers (IEnumerable<string> refNumbers)
    {
        if (!refNumbers.Any())
            return null;

        return $", Member Reference Number: {string.Join(", ", refNumbers)}";
    }
}