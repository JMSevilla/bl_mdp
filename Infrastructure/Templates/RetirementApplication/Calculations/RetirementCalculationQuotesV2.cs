using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LanguageExt;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.MdpApi;
using WTW.MdpService.Infrastructure.Templates.Common;
using WTW.MdpService.Infrastructure.Templates.Common.CmsContentModels;
using WTW.MdpService.Infrastructure.Templates.GenericJourneys;
using WTW.MdpService.Journeys.Submit.Services.DocumentsRenderer;
using WTW.MdpService.Retirement;
using WTW.Web.Serialization;

namespace WTW.MdpService.Infrastructure.Templates.RetirementApplication.Calculations;

public class RetirementCalculationQuotesV2 : CmsTemplateContentJsonParser, IRetirementCalculationQuotesV2
{
    private readonly ICalculationsParser _calculationsParser;
    private readonly IRetirementService _retirementService;
    private readonly IMdpClient _mdpClient;
    private readonly ICmsDataParser _cmsDataParser;

    public RetirementCalculationQuotesV2(
        ICalculationsParser calculationsParser,
        IRetirementService retirementService,
        IMdpClient mdpClient,
        ICmsDataParser cmsDataParser) : base(mdpClient, cmsDataParser)
    {
        _calculationsParser = calculationsParser;
        _retirementService = retirementService;
        _mdpClient = mdpClient;
        _cmsDataParser = cmsDataParser;
    }

    public async Task<(Dictionary<string, object>, List<SummaryBlock>)> Create(
       Calculation calculation,
       string selectedQuoteName,
       JsonElement summary)
    {
        var retirementV2 = _calculationsParser.GetRetirementV2(calculation.RetirementJsonV2);
        var optionsDictionary = _retirementService.GetSelectedQuoteDetails(selectedQuoteName, retirementV2);

        var summaryBlocks = await GetSummaryBlocks(summary, optionsDictionary);
        return (optionsDictionary, summaryBlocks);
    }

    public async Task<(Dictionary<string, object>, List<SummaryBlock>)> Create(
        Calculation calculation,
        string selectedQuoteName,
        JsonElement summary,
        (string AccessToken, string Env, string Bgroup) auth)
    {
        var retirementV2 = _calculationsParser.GetRetirementV2(calculation.RetirementJsonV2);
        var optionsDictionary = _retirementService.GetSelectedQuoteDetails(selectedQuoteName, retirementV2);
        var uris = _cmsDataParser.GetDataSummaryBlockSourceUris(summary);
        var props = await _mdpClient.GetData(uris, auth);
        var dataJson = JsonSerializer.Serialize(props, SerialiationBuilder.Options());
        var dataValues = new DataSummaryBlockValues().Create(dataJson);

        var allProps = optionsDictionary.Concat(dataValues)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var summaryBlocks = await GetSummaryBlocks(summary, allProps);
        return (optionsDictionary, summaryBlocks);
    }

    public List<ContentBlockItem> GetContentBlocks(IEnumerable<JsonElement> contentBlocks, CmsTokenInformationResponse cmsToken)
    {
        var parcedContentBlocks = GetContentBlock(contentBlocks).ToList();
        var updatedContentBlocks = ReplaceCmsTokens(parcedContentBlocks, cmsToken);
        return updatedContentBlocks;
    }

    public Option<OptionBlock> FilterOptionsByKey(JsonElement options, Calculation calculation, string key)
    {
        var summaryItems = new List<SummaryItem>();
        var retirementV2 = _calculationsParser.GetRetirementV2(calculation.RetirementJsonV2);
        var optionsDictionary = _retirementService.GetSelectedQuoteDetails(key, retirementV2);

        foreach (var option in options.EnumerateArray())
        {
            if (option.TryGetProperty("elements", out var elements))
            {
                if (elements.TryGetProperty("key", out var keyElement))
                {
                    if (keyElement.TryGetProperty("value", out var keyValue) && keyValue.GetString() == key)
                    {
                        string header = elements.GetProperty("header").GetProperty("value").GetString();
                        string description = elements.GetProperty("description").GetProperty("value").GetString();
                        int? orderNo = elements.GetProperty("orderNo").GetProperty("value").GetInt32();

                        summaryItems = GetSummaryItems(elements, optionsDictionary);
                        return new OptionBlock
                        {
                            Description = description,
                            Header = header,
                            OrderNo = orderNo,
                            SummaryItems = summaryItems
                        };
                    }
                }
            }
        }
        return Option<OptionBlock>.None;
    }
}
