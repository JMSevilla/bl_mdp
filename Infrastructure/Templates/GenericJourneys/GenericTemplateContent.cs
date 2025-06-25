using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Threading.Tasks;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.Journeys;
using WTW.MdpService.Infrastructure.MdpApi;
using WTW.MdpService.Infrastructure.Templates.Common;
using WTW.MdpService.Infrastructure.Templates.Common.CmsContentModels;
using WTW.MdpService.Journeys;
using WTW.MdpService.Journeys.Submit.Services.DocumentsRenderer;
using WTW.MdpService.Retirement;
using WTW.Web.Serialization;

namespace WTW.MdpService.Infrastructure.Templates.GenericJourneys;

public class GenericTemplateContent : CmsTemplateContentJsonParser, IGenericTemplateContent
{
    private readonly IGenericJourneyDetails _genericJourneyDetails;
    private readonly ILogger<GenericTemplateContent> _logger;

    public GenericTemplateContent(IMdpClient mdpClient,
        ICmsDataParser cmsDataParser,
        IGenericJourneyDetails genericJourneyDetails,
        ILogger<GenericTemplateContent> logger)
        : base(mdpClient, cmsDataParser)
    {
        _genericJourneyDetails = genericJourneyDetails;
        _logger = logger;
    }

    public async Task<List<DataSummaryItem>> GetDataSummaryBlocks(IEnumerable<DataSummaryBlock> summaries, ExpandoObject obj, (string AccessToken, string Env, string Bgroup) auth)
    {
        var result = new List<DataSummaryItem>();

        foreach (var summaryItem in summaries)
        {
            var blocks = await GetDataSummaryBlocks(summaryItem.DataSummaryJsonElement, obj, auth);
            result.Add(new DataSummaryItem { Key = summaryItem.Key, SummaryBlocks = blocks });
        }

        return result;
    }

    public async Task<List<SummaryBlock>> GetDataSummaryBlocks(JsonElement summary, ExpandoObject obj, (string AccessToken, string Env, string Bgroup) auth)
    {
        _logger.LogInformation("Parsing summary blocks from cms for generic journey");

        var dataJson = JsonSerializer.Serialize(obj, SerialiationBuilder.Options());
        var dataValues = new DataSummaryBlockValues().Create(dataJson);
        var summaryBlocks = await GetSummaryBlocks(summary, dataValues, auth);

        _logger.LogInformation("Parsed {summaryBlocksCount} summary blocks from cms for generic journey", summaryBlocks.Count);
        return summaryBlocks;
    }

    public IEnumerable<ContentBlockItem> GetContentBlockItems(IEnumerable<JsonElement> contentBlocks, ExpandoObject obj, (string AccessToken, string Env, string Bgroup) auth)
    {
        _logger.LogInformation("Parsing content blocks from cms for generic journey");

        var dataJson = JsonSerializer.Serialize(obj, SerialiationBuilder.Options());
        var dataValues = new DataSummaryBlockValues().Create(dataJson);

        var contentBlockItems = GetContentBlock(contentBlocks);
        var updatedContentBlocks = ReplaceCmsTokens(contentBlockItems, dataValues);
        return updatedContentBlocks;
    }

    protected override string GetTokenOrCalcApiValue(JsonElement summaryItemElementProperty, Dictionary<string, object> props)
    {
        var showBlock = GetOptionalPropertyValueAsString(summaryItemElementProperty, "showDespiteEmpty");
        var tokenValue = FindtTokenOrCalcApiValue(summaryItemElementProperty, props);
        if (showBlock == "True" && tokenValue == "")
            return "-";

        return tokenValue;
    }

    private string FindtTokenOrCalcApiValue(JsonElement element, Dictionary<string, object> props)
    {
        var valueToken = GetPropertyValue(element, "value").ToString();
        var isQuoteValue = props.TryGetValue(valueToken, out var quotev2Value);
        if (!isQuoteValue)
            return valueToken.Contains("journey.") ? string.Empty : valueToken;

        return quotev2Value.ToString();
    }
}
