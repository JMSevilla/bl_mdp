using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Infrastructure.CasesApi;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Journeys.Submit.Services.Dto;
using WTW.Web.Extensions;
using WTW.Web.LanguageExt;
using WTW.Web.Serialization;

namespace WTW.MdpService.Journeys.Submit.Services;

public class CaseRequestFactory : ICaseRequestFactory
{
    private static readonly ICollection<string> _supportedQuoteRequestsCaseTypes = new List<string> { "retirement", "transfer" };
    private readonly IJourneysRepository _journeysRepository;
    private readonly IJsonConversionService _jsonConverter;
    private readonly ILogger<ICaseRequestFactory> _logger;

    public CaseRequestFactory(IJourneysRepository journeysRepository, IJsonConversionService jsonConverter, ILogger<ICaseRequestFactory> logger)
    {
        _journeysRepository = journeysRepository;
        _jsonConverter = jsonConverter;
        _logger = logger;
    }

    public CreateCaseRequest CreateForGenericRetirement(string businessGroup, string referenceNumber, string caseCode = "RTP9")
    {      
        return new CreateCaseRequest
        {
            BusinessGroup = businessGroup,
            ReferenceNumber = referenceNumber,
            CaseCode = caseCode,
            BatchSource = "MDP",
            BatchDescription = "Case created by an online application",
            Narrative = "",
            Notes = $"Case created by an online application",
            StickyNotes = "Case created by an online retirement application"
        };
    }

    public async Task<Either<Error, CreateCaseRequest>> CreateForQuoteRequest(string businessGroup, string referenceNumber, string caseType)
    {
        if (!_supportedQuoteRequestsCaseTypes.Any(t => t.Equals(caseType, StringComparison.InvariantCultureIgnoreCase)))
        {
            _logger.LogError("Quote request case type \\\"{CaseType}\\\" is not supported", caseType);
            return Error.New($"Quote request case type \"{caseType}\" is not supported.");
        }

        if (caseType.Equals("retirement", StringComparison.InvariantCultureIgnoreCase))
            return await CreateRetirementRequest(businessGroup, referenceNumber);

        return CreateTransferRequest(businessGroup, referenceNumber);
    }

    private async Task<Either<Error, CreateCaseRequest>> CreateRetirementRequest(string businessGroup, string referenceNumber)
    {
        var date = await GetRetirementDate(businessGroup, referenceNumber);
        if (date.IsLeft)
            return date.Left();

        var dateString = date.Right().ToString("d", CultureInfo.CreateSpecificCulture("en-GB"));

        return CreateCaseRequest(businessGroup, referenceNumber, "RTQ9", dateString);
    }

    private CreateCaseRequest CreateTransferRequest(string businessGroup, string referenceNumber)
    {
        var dateString = DateTime.UtcNow.ToString("d", CultureInfo.CreateSpecificCulture("en-GB"));
        return CreateCaseRequest(businessGroup, referenceNumber, "TOQ9", dateString);
    }

    private CreateCaseRequest CreateCaseRequest(string businessGroup, string referenceNumber, string caseCode, string dateString)
    {
        return new CreateCaseRequest
        {
            BusinessGroup = businessGroup,
            ReferenceNumber = referenceNumber,
            CaseCode = caseCode,
            BatchSource = "MDP",
            BatchDescription = "Case created by an online application",
            Narrative = "",
            Notes = $"Assure Calc fatal at {dateString}",
            StickyNotes = $"Assure Calc fatal at {dateString}"
        };
    }

    private async Task<Either<Error, DateTime>> GetRetirementDate(string businessGroup, string referenceNumber)
    {
        var journey = await _journeysRepository.Find(businessGroup, referenceNumber, "requestquote");
        if (journey.IsNone)
        {
            _logger.LogError("Member: {businessGroup} {referenceNumber} have not started journey with type: \"requestquote\".", businessGroup, referenceNumber);
            return Error.New($"Member: {businessGroup} {referenceNumber} does not have started journey with type: \"requestquote\".");
        }

        var step = journey.Value().GetStepByKey("quote_choose_ret_date");
        if (step.IsNone)
        {
            _logger.LogError("Member: {businessGroup} {referenceNumber} does not have step \"quote_choose_ret_date\" within journey type: \"requestquote\".", businessGroup, referenceNumber);
            return Error.New($"Member: {businessGroup} {referenceNumber} does not have step \"quote_choose_ret_date\" within journey type: \"requestquote\".");
        }

        var genericDataByKey = step.Value().GetGenericDataByKey("date_picker_with_age_");
        if (genericDataByKey.IsNone)
        {
            _logger.LogError("Member: {businessGroup} {referenceNumber} does not have generic data with key \"date_picker_with_age_\" for step \"quote_choose_ret_date\" within journey type: \"requestquote\".",
                businessGroup, referenceNumber);
            return Error.New($"Member: {businessGroup} {referenceNumber} does not have generic data with key \"date_picker_with_age_\" for step \"quote_choose_ret_date\" within journey type: \"requestquote\".");
        }

        var genericDataJson = genericDataByKey.Value().GenericDataJson;
        var genericDataDto = _jsonConverter.Deserialize<GenericDataForRetirmentDate>(genericDataJson);
        if (!genericDataDto.SelectedDate.HasValue)
        {
            _logger.LogError($"Member: {businessGroup} {referenceNumber}. Unable parse retirement date from generic data.", businessGroup, referenceNumber);
            return Error.New($"Member: {businessGroup} {referenceNumber}. Unable parse retirement date from generic data.");
        }

        return genericDataDto.SelectedDate.Value;
    }
}