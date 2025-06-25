using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.Web.Extensions;

namespace WTW.MdpService.Infrastructure.Calculations;

public class RetirementV2Mapper
{
    public RetirementV2 MapToDomain(RetirementResponseV2 retirementResponse, string eventType)
    {
        var wordingFlags = retirementResponse.Results.Mdp.WordingFlags.Concat(TrancheIncreaseMethodsWordingFlags(retirementResponse.Results.Mdp.TrancheIncreaseMethods));
        var quotesV2 = ParseQuotesV2(retirementResponse.Results.Mdp);
        var retirementV2Params = new RetirementV2Params
        {
            EventType = eventType,
            DateOfBirth = retirementResponse.Results.Mdp.DateOfBirth,
            DatePensionableServiceCommenced = retirementResponse.Results.Mdp.DatePensionableServiceCommenced,
            DateOfLeaving = retirementResponse.Results.Mdp.DateOfLeaving,
            StatePensionDate = retirementResponse.Results.Mdp.StatePensionDate,
            StatePensionDeduction = retirementResponse.Results.Mdp.StatePensionDeduction,
            GMPAge = retirementResponse.Results.Mdp.GMPAge,
            Post88GMPIncreaseCap = retirementResponse.Results.Mdp.Post88GMPIncreaseCap,
            Pre88GMPAtGMPAge = retirementResponse.Results.Mdp.Pre88GMPAtGMPAge,
            Post88GMPAtGMPAge = retirementResponse.Results.Mdp.Post88GMPAtGMPAge,
            TransferInService = retirementResponse.Results.Mdp.TransferInService,
            TotalPensionableService = retirementResponse.Results.Mdp.TotalPensionableService,
            FinalPensionableSalary = retirementResponse.Results.Mdp.FinalPensionableSalary,
            InternalAVCFundValue = retirementResponse.Results.Mdp.InternalAVCFundValue,
            ExternalAvcFundValue = retirementResponse.Results.Mdp.ExternalAvcFundValue,
            TotalAvcFundValue = retirementResponse.Results.Mdp.TotalAvcFundValue,
            TotalFundValue = retirementResponse.Results.Mdp.TotalFundValue,
            StandardLifetimeAllowance = retirementResponse.Results.Mdp.StandardLifetimeAllowance,
            TotalLtaUsedPercentage = retirementResponse.Results.Mdp.TotalLtaUsedPercentage,
            MinimumPermittedTotalLumpSum = retirementResponse.Results.Mdp.MinimumPermittedTotalLumpSum,
            MaximumPermittedTotalLumpSum = retirementResponse.Results.Mdp.MaximumPermittedTotalLumpSum,
            MaximumPermittedStandardLumpSum = retirementResponse.Results.Mdp.MaximumPermittedStandardLumpSum,
            TotalLtaRemainingPercentage = retirementResponse.Results.Mdp.TotalLtaRemainingPercentage,
            NormalMinimumPensionAge = retirementResponse.Results.Mdp.StatutoryFactors?.NormalMinimumPensionAge,
            QuotesV2 = quotesV2,
            WordingFlags = wordingFlags,
            InputEffectiveDate = retirementResponse.Inputs?.EffectiveDate,
            ResidualFundValue = retirementResponse.Results.Mdp.ResidualFundValue,
        };

        return new RetirementV2(retirementV2Params);
    }

    public RetirementV2 MapFromDto(RetirementDtoV2 dto)
    {
        var quotesV2 = dto.QuotesV2.Select(x =>
            new QuoteV2(
                x.Name,
                x.Attributes.Select(a => new QuoteAttributesV2(a.Name, a.Value)),
                x.PensionTranches.Select(y => new PensionTranche(y.TrancheTypeCode, y.Value, y.IncreaseTypeCode))));

        var retParams = new RetirementV2Params
        {
            EventType = dto.CalculationEventType,
            DateOfBirth = dto.DateOfBirth,
            DatePensionableServiceCommenced = dto.DatePensionableServiceCommenced,
            DateOfLeaving = dto.DateOfLeaving,
            StatePensionDate = dto.StatePensionDate,
            StatePensionDeduction = dto.StatePensionDeduction,
            GMPAge = dto.GMPAge,
            Post88GMPIncreaseCap = dto.Post88GMPIncreaseCap,
            Pre88GMPAtGMPAge = dto.Pre88GMPAtGMPAge,
            Post88GMPAtGMPAge = dto.Post88GMPAtGMPAge,
            TransferInService = dto.TransferInService,
            TotalPensionableService = dto.TotalPensionableService,
            FinalPensionableSalary = dto.FinalPensionableSalary,
            InternalAVCFundValue = dto.InternalAVCFundValue,
            ExternalAvcFundValue = dto.ExternalAvcFundValue,
            TotalAvcFundValue = dto.TotalAvcFundValue,
            TotalFundValue = dto.TotalFundValue,
            StandardLifetimeAllowance = dto.StandardLifetimeAllowance,
            TotalLtaUsedPercentage = dto.TotalLtaUsedPercentage,
            MinimumPermittedTotalLumpSum = dto.MinimumPermittedTotalLumpSum,
            MaximumPermittedTotalLumpSum = dto.MaximumPermittedTotalLumpSum,
            MaximumPermittedStandardLumpSum = dto.MaximumPermittedStandardLumpSum,
            TotalLtaRemainingPercentage = dto.TotalLtaRemainingPercentage,
            NormalMinimumPensionAge = dto.NormalMinimumPensionAge,
            QuotesV2 = quotesV2,
            WordingFlags = dto.WordingFlags,
            InputEffectiveDate = dto.InputEffectiveDate,
            ResidualFundValue = dto.ResidualFundValue,
        };

        return new RetirementV2(retParams);
    }

    private IEnumerable<QuoteV2> ParseQuotesV2(MdpResponseV2 mdp)
    {
        var allOptions = NewOptionsObject(mdp.Options);
        var quotes = allOptions.EnumerateObject().Select(x => ParseAllAttributes(x.Name, x.Value, mdp.TrancheIncreaseMethods)).ToList();
        return quotes;
    }

    private JsonElement NewOptionsObject(JsonElement option)
    {
        return GetNewObject(ParseOptions(option));
    }

    private Dictionary<string, object> ParseOptions(JsonElement option, string rootOptionName = null)
    {
        var options = new Dictionary<string, object>();

        foreach (var item in option.EnumerateObject())
        {
            var fullItemName = rootOptionName != null ? rootOptionName + "_" + item.Name : item.Name;

            if (!item.Value.TryGetProperty("options", out var opt))
            {
                options.Add(fullItemName, item.Value);
                continue;
            }

            options.Add(fullItemName, item.Value);

            var deeperOptions = ParseOptions(opt, fullItemName);
            options.Append<string, object>(deeperOptions);
        }

        return options;
    }

    private QuoteV2 ParseAllAttributes(string name, JsonElement value, JsonElement? trancheIncreaseMethods)
    {
        if (!value.TryGetProperty("attributes", out var attribute))
            return new QuoteV2(name, new List<QuoteAttributesV2>(), new List<PensionTranche>());

        var attributeValues = attribute.EnumerateObject();

        if (!attribute.TryGetProperty("pensionTranches", out var pensionTranche))
        {
            var attributes = attributeValues.Map(x => new QuoteAttributesV2(x.Name, decimal.TryParse(x.Value.ToString(), out decimal attributeValue) ? attributeValue : null)).ToList();
            return new QuoteV2(name, attributes, new List<PensionTranche>());
        }

        var attributesOnly = attributeValues.Where(x => !x.NameEquals("pensionTranches"));
        return new QuoteV2(name, attributesOnly.Map(x => new QuoteAttributesV2(x.Name, decimal.TryParse(x.Value.ToString(), out decimal attributeValue) ? attributeValue : null)).ToList(), ParsePensionTranches(pensionTranche, trancheIncreaseMethods).ToList());
    }

    private IEnumerable<PensionTranche> ParsePensionTranches(JsonElement pensionTranches, JsonElement? trancheIncreaseMethods)
    {
        return pensionTranches.EnumerateObject()
            .Where(x => !x.NameEquals("total"))
            .Select(x => new PensionTranche(
                x.Name,
                decimal.Parse(x.Value.ToString()),
                trancheIncreaseMethods.Value.EnumerateObject().FirstOrDefault(y => y.NameEquals(x.Name)).Value.ToString()));
    }

    private JsonElement GetNewObject(Dictionary<string, object> propertiesToRetain)
    {
        return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(propertiesToRetain));
    }

    private List<string> TrancheIncreaseMethodsWordingFlags(JsonElement? trancheIncreaseMethods)
    {
        if (trancheIncreaseMethods == null || trancheIncreaseMethods.Value.ValueKind == JsonValueKind.Undefined)
            return new List<string>();

        return trancheIncreaseMethods.Value.EnumerateObject().Select(x => "tranche_" + x.Name + "_" + x.Value.ToString()).ToList();
    }
}