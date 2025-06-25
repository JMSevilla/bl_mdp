using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.Web.LanguageExt;
using WTW.Web.Serialization;

namespace WTW.MdpService.Infrastructure.Journeys;

public class GenericJourneyDetails : IGenericJourneyDetails
{
    private readonly IRetirementService _retirementService;
    private readonly IJourneysRepository _journeysRepository;
    private readonly IRetirementJourneyRepository _retirementJourneyRepository;
    private readonly ICalculationsParser _calculationsParser;
    private readonly IJsonConversionService _jsonConversionService;

    public GenericJourneyDetails(IRetirementService retirementService,
        IJourneysRepository journeysRepository,
        IRetirementJourneyRepository retirementJourneyRepository,
        ICalculationsParser calculationsParser,
        IJsonConversionService jsonConversionService)
    {
        _retirementService = retirementService;
        _journeysRepository = journeysRepository;
        _retirementJourneyRepository = retirementJourneyRepository;
        _calculationsParser = calculationsParser;
        _jsonConversionService = jsonConversionService;
    }

    public GenericJourneyData GetAll(GenericJourney genericJourney)
    {
        return new GenericJourneyData
        {
            Type = genericJourney.Type,
            Status = genericJourney.Status,
            ExpirationDate = genericJourney.ExpirationDate,
            PreJourneyData = GetJourneyStepData(genericJourney.GetFirstStep().JourneyGenericDataList),
            StepsWithData = GetJourneyStepWithData(genericJourney.GetJourneyStepsWithGenericData()),
            StepsWithQuestion = GetStepsWithQuestions(genericJourney),
            StepsWithCheckboxes = GetStepsWithCheckboxLists(genericJourney.GetStepsWithCheckboxLists())
        };
    }

    public Task<Option<GenericJourneyData>> GetAll(string businessGroup, string referenceNumber, string journeyType)
    {
        return (journeyType.ToLower()) switch
        {
            "dbretirementapplication" => GetRetirementJourneyData(businessGroup, referenceNumber),
            _ => GetGenericJourneyData(businessGroup, referenceNumber, journeyType),
        };
    }

    private async Task<Option<GenericJourneyData>> GetRetirementJourneyData(string businessGroup, string referenceNumber)
    {
        var retirementJourney = await _retirementJourneyRepository.Find(businessGroup, referenceNumber);
        if (retirementJourney.IsNone)
            return Option<GenericJourneyData>.None;

        var retirementV2 = _calculationsParser.GetRetirementV2(retirementJourney.Value().Calculation.RetirementJsonV2);
        var selectedQuoteDetails = _retirementService.GetSelectedQuoteDetails(retirementJourney.Value().Calculation.SelectedQuoteName, retirementV2);
        selectedQuoteDetails.Add("selectedQuoteFullName", retirementJourney.Value().Calculation.SelectedQuoteName);

        var preJourneyData = new List<JourneyGenericData>
        {
            new JourneyGenericData(
                _jsonConversionService.Serialize(
                    new
                    {
                        CaseNumber = retirementJourney.Value().CaseNumber,
                        RetirementDate = retirementJourney.Value().MemberQuote.SearchedRetirementDate
                    }),
                "JourneySubmissionDetails"),
            new JourneyGenericData(
                _jsonConversionService.Serialize(selectedQuoteDetails),
                "SelectedQuoteDetails")
        };

        var checkBoxLists = GetRetirementCheckboxListData(retirementJourney)
            .Concat(retirementJourney.Value().GetStepsWithCheckboxLists());
        var stepsWithGenericData = GetRetirementJourneyGenericData(retirementJourney)
            .Concat(retirementJourney.Value().GetJourneyStepsWithGenericData());

        return new GenericJourneyData()
        {
            Type = "dbretirementapplication",
            Status = retirementJourney.Value().SubmissionDate == null ? "StartedRA" : "SubmittedRA",
            ExpirationDate = retirementJourney.Value().ExpirationDate,
            PreJourneyData = GetJourneyStepData(preJourneyData),
            StepsWithData = GetJourneyStepWithData(stepsWithGenericData),
            StepsWithQuestion = GetStepsWithQuestions(retirementJourney.Value()),
            StepsWithCheckboxes = GetStepsWithCheckboxLists(ConvertAnswersToYesNo(checkBoxLists))
        };
    }

    private static List<(string PageKey, Dictionary<string, List<Checkbox>> Checkboxes)> GetRetirementCheckboxListData(Option<Domain.Mdp.RetirementJourney> retirementJourney)
    {
        return new List<(string PageKey, Dictionary<string, List<Checkbox>> Checkboxes)>
            {
            ("pw_guidance_d",
            new Dictionary<string, List<Checkbox>>{ { "pw_opt_out_form", new List<Checkbox> { new Checkbox("optOutPensionWise", retirementJourney.Value().OptOutPensionWise.HasValue && retirementJourney.Value().OptOutPensionWise.Value) } } }),
            ("submit_retirement_app",
            new Dictionary<string, List<Checkbox>>{ { "retirement_application_acknowledgements", new List<Checkbox> { new Checkbox("acknowledgeFinAdvisor", retirementJourney.Value().AcknowledgeFinancialAdvisor.HasValue && retirementJourney.Value().AcknowledgeFinancialAdvisor.Value) } } }),
            };
    }

    private List<JourneyStep> GetRetirementJourneyGenericData(Option<Domain.Mdp.RetirementJourney> retirementJourney)
    {
        var ltaSummaryAmountStep = JourneyStep.Create("lta_summary_amount", null, DateTimeOffset.UtcNow);
        ltaSummaryAmountStep.UpdateGenericData("journey_continue_control", _jsonConversionService.Serialize(
                    new
                    {
                        retirementJourney.Value().EnteredLtaPercentage,
                    }));

        var pensionWiseDateStep = JourneyStep.Create("pw_guidance_a", null, DateTimeOffset.UtcNow);
        pensionWiseDateStep.UpdateGenericData("pension_wise_date_form", _jsonConversionService.Serialize(
                    new
                    {
                        retirementJourney.Value().PensionWiseDate,
                    }));
        var financialAdviseDateStep = JourneyStep.Create("pw_guidance_c", null, DateTimeOffset.UtcNow);
        financialAdviseDateStep.UpdateGenericData("financial_advise_date_form", _jsonConversionService.Serialize(
                    new
                    {
                        retirementJourney.Value().FinancialAdviseDate,
                    }));

        var steps = new List<JourneyStep>();

        if (retirementJourney.Value().GetStepByKey("lta_summary_amount").IsSome)
            steps.Add(ltaSummaryAmountStep);

        if (retirementJourney.Value().GetStepByKey("pw_guidance_a").IsSome)
            steps.Add(pensionWiseDateStep);
        
        if (retirementJourney.Value().GetStepByKey("pw_guidance_c").IsSome)
            steps.Add(financialAdviseDateStep);

        return steps;
    }

    private async Task<Option<GenericJourneyData>> GetGenericJourneyData(string businessGroup, string referenceNumber, string journeyType)
    {
        var genericJourney = await _journeysRepository.Find(businessGroup, referenceNumber, journeyType);
        if (genericJourney.IsNone)
            return Option<GenericJourneyData>.None;

        return GetAll(genericJourney.Value());
    }

    private IDictionary<string, object> GetStepsWithCheckboxLists(IEnumerable<(string PageKey, Dictionary<string, List<Checkbox>> Checkboxes)> checkBoxesLists)
    {
        var result = (IDictionary<string, object>)new ExpandoObject();

        foreach (var step in checkBoxesLists)
        {
            result.Add(step.PageKey, GetCheckboxListData(step.Checkboxes));
        }
        return result;
    }

    private IDictionary<string, object> GetCheckboxListData(Dictionary<string, List<Checkbox>> checkboxList)
    {
        var result = (IDictionary<string, object>)new ExpandoObject();

        foreach (var list in checkboxList)
        {
            result.Add(list.Key, GetCheckboxData(list.Value));
        }
        return result;
    }

    private IDictionary<string, object> GetCheckboxData(List<Checkbox> checkboxes)
    {
        var result = (IDictionary<string, object>)new ExpandoObject();

        foreach (var checkbox in checkboxes)
        {
            result.Add(checkbox.Key, checkbox.Answer != null ? new { checkbox.AnswerValue, checkbox.Answer } : new { checkbox.AnswerValue });
        }
        return result;
    }

    private IDictionary<string, object> GetStepsWithQuestions(Journey genericJourney)
    {
        var result = (IDictionary<string, object>)new ExpandoObject();

        foreach (var step in genericJourney.GetStepsWithQuestionForms())
        {
            result.Add(step.CurrentPageKey, GetQuestionData(step.QuestionForm));
        }
        return result;
    }

    private IDictionary<string, object> GetQuestionData(QuestionForm questionForm)
    {
        var result = (IDictionary<string, object>)new ExpandoObject();
        result.Add(questionForm.QuestionKey, new { questionForm.AnswerKey, questionForm.AnswerValue });
        return result;
    }

    private IDictionary<string, object> GetJourneyStepWithData(IEnumerable<JourneyStep> stepsWithGenericData)
    {
        var result = (IDictionary<string, object>)new ExpandoObject();
        foreach (var step in stepsWithGenericData)
        {
            result.Add(step.CurrentPageKey, GetJourneyStepData(step.JourneyGenericDataList));
        }

        return result;
    }

    private IDictionary<string, object> GetJourneyStepData(IEnumerable<JourneyGenericData> dataList)
    {
        var result = (IDictionary<string, object>)new ExpandoObject();
        foreach (var genericData in dataList)
        {
            var value = JsonSerializer.Deserialize<JsonElement>(genericData.GenericDataJson, SerialiationBuilder.Options());
            Dictionary<string, object> address;
            if (value.TryGetProperty("address", out var property) && (address = GedAddressObject(property)).Any())
            {
                result.Add(genericData.FormKey, new { address = address });
                continue;
            }

            if(value.TryGetProperty("surname", out var surname) && value.TryGetProperty("name", out var name))
            {
                result.Add(genericData.FormKey, new { name = name, surname = surname, fullName = name.GetString() + " " + surname.GetString() });
                continue;
            }

            result.Add(genericData.FormKey, value);
        }

        return result;
    }

    private static Dictionary<string, object> GedAddressObject(JsonElement property)
    {
        var dict = new Dictionary<string, object>();
        if (property.TryGetProperty("countryName", out var countryName))
            dict.Add("country", countryName.GetString());

        if (property.TryGetProperty("countryCode", out var countryCode))
            dict.Add("countryCode", countryCode.GetString());

        if (property.TryGetProperty("postCode", out var postCode))
            dict.Add("postCode", postCode.GetString());

        var linesList = new List<string>();
        for (int i = 1; i <= 5; i++)
        {
            if (property.TryGetProperty($"line{i}", out var line))
                linesList.Add(line.GetString());
        }

        if (linesList.Any())
            dict.Add("lines", linesList.Where(x => !string.IsNullOrWhiteSpace(x)));

        return dict;
    }

    private static IEnumerable<(string PageKey, Dictionary<string, List<Checkbox>> Checkboxes)> ConvertAnswersToYesNo(IEnumerable<(string PageKey, Dictionary<string, List<Checkbox>> Checkboxes)> checkBoxesLists)
    {
        var convertedCheckBoxesLists = new List<(string PageKey, Dictionary<string, List<Checkbox>> Checkboxes)>();

        foreach (var checkBoxesList in checkBoxesLists)
        {
            var convertedCheckboxes = new Dictionary<string, List<Checkbox>>();

            foreach (var checkboxList in checkBoxesList.Checkboxes)
            {
                var convertedCheckboxesList = new List<Checkbox>();

                foreach (var checkbox in checkboxList.Value)
                {
                    var convertedCheckbox = new Checkbox(checkbox.Key, checkbox.AnswerValue, checkbox.AnswerValue ? "Yes" : "No");
                    convertedCheckboxesList.Add(convertedCheckbox);
                }

                convertedCheckboxes.Add(checkboxList.Key, convertedCheckboxesList);
            }

            convertedCheckBoxesLists.Add((checkBoxesList.PageKey, convertedCheckboxes));
        }

        return convertedCheckBoxesLists;
    }
}
