using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WTW.MdpService.Infrastructure.MdpApi;
using WTW.MdpService.Infrastructure.Templates.Common.CmsContentModels;
using WTW.MdpService.Journeys.Submit.Services.DocumentsRenderer;
using WTW.MdpService.Retirement;
using WTW.Web.Extensions;
using WTW.Web.Templates;

namespace WTW.MdpService.Infrastructure.Templates.Common;

public class CmsTemplateContentJsonParser
{
    private readonly IMdpClient _mdpClient;
    private readonly ICmsDataParser _cmsDataParser;

    public CmsTemplateContentJsonParser(IMdpClient mdpClient, ICmsDataParser cmsDataParser)
    {
        _mdpClient = mdpClient;
        _cmsDataParser = cmsDataParser;
    }

    public async Task<List<SummaryBlock>> GetSummaryBlocks(JsonElement element, Dictionary<string, object> props, (string AccessToken, string Env, string Bgroup) auth = default)
    {
        var summaryBlocks = new List<SummaryBlock>();

        if (element.ValueKind == JsonValueKind.Undefined)
            return summaryBlocks;

        if (!element.TryGetProperty(ContentPropertyName.Elements, out var elementsProperty))
            return summaryBlocks;

        var values = TryGetPropertyValue(elementsProperty, ContentPropertyName.SummaryBlocks);
        foreach (var summaryElement in values.EnumerateArray())
        {
            summaryElement.TryGetProperty(ContentPropertyName.Elements, out var summaryProperty);

            var result = new SummaryBlock
            (
                Header: GetPropertyValue(summaryProperty, ContentPropertyName.Header).ToString(),
                BottomInformation: await GetBottomInformation(summaryProperty, ContentPropertyName.BottomInformation, auth)
            );

            result.SummaryItems.AddRange(GetSummaryItems(summaryProperty, props));
            summaryBlocks.Add(result);
        }
        return summaryBlocks;
    }

    public List<SummaryItem> GetSummaryItems(JsonElement summaryProperty, Dictionary<string, object> props)
    {
        var result = new List<SummaryItem>();
        var summaryValues = TryGetPropertyValue(summaryProperty, ContentPropertyName.SummaryItems);
        if (summaryValues.ValueKind != JsonValueKind.Array)
            return result;

        foreach (var summaryItem in summaryValues.EnumerateArray())
        {
            summaryItem.TryGetProperty(ContentPropertyName.Elements, out var summaryItemElementProperty);
            var summaryItemResult = new SummaryItem
            (
                Header: GetPropertyValue(summaryItemElementProperty, ContentPropertyName.Header).ToString(),
                Format: GetPropertyByName(GetPropertyValue(summaryItemElementProperty, ContentPropertyName.Format), ContentPropertyName.Selection).Value.ToString(),
                Divider: GetPropertyValue(summaryItemElementProperty, ContentPropertyName.Divider).ToString(),
                Description: GetDescriptionTokens(summaryItemElementProperty, ContentPropertyName.Description, props),
                Value: GetTokenOrCalcApiValue(summaryItemElementProperty, props)
            );

            var explanationSummaryItems = GetPropertyByName(summaryItemElementProperty, ContentPropertyName.ExplanationSummaryItems);
            explanationSummaryItems.Value.TryGetProperty(ContentPropertyName.Values, out var explanationSummaryValues);

            if (explanationSummaryValues.ValueKind == JsonValueKind.Array)
            {
                foreach (var explanationSummValue in explanationSummaryValues.EnumerateArray())
                {
                    explanationSummValue.TryGetProperty(ContentPropertyName.Elements, out var explanationValueElementProperties);
                    summaryItemResult.ExplanationSummaryItems.Add
                    (
                        new ExplanationSummaryItem
                        (
                            Header: GetPropertyValue(explanationValueElementProperties, ContentPropertyName.Header).ToString(),
                            Format: GetPropertyByName(GetPropertyValue(explanationValueElementProperties, ContentPropertyName.Format), ContentPropertyName.Selection).Value.ToString(),
                            Description: GetPropertyValue(explanationValueElementProperties, ContentPropertyName.Description).ToString(),
                            Value: GetExplanationSummaryItemValue(explanationValueElementProperties, props)
                        )
                    );
                }
            }

            result.Add(summaryItemResult);
        }

        return result;
    }

    public IEnumerable<ContentBlockItem> GetContentBlock(IEnumerable<JsonElement> contentBlocks)
    {
        var result = new List<ContentBlockItem>();

        foreach (var contentBlock in contentBlocks)
        {
            foreach (var contentBlockProperty in contentBlock.EnumerateArray())
            {
                contentBlockProperty.TryGetProperty(ContentPropertyName.Elements, out var elementProperty);
                var contentProperty = GetPropertyByName(elementProperty, ContentPropertyName.Content);
                contentProperty.Value.TryGetProperty(ContentPropertyName.Value, out var contentValue);
                var blockItem = new ContentBlockItem
                (
                    Key: GetPropertyValue(elementProperty, ContentPropertyName.ContentBlockKey).ToString(),
                    Header: GetPropertyValue(elementProperty, ContentPropertyName.Header).ToString(),
                    Value: contentValue.ToString()
                );

                result.Add(blockItem);
            }
        }
        return result;
    }

    public List<ContentBlockItem> ReplaceCmsTokens(IEnumerable<ContentBlockItem> contentBlockItems, CmsTokenInformationResponse cmsToken)
    {
        var result = new List<ContentBlockItem>();
        var cmsTokenValuesDict = cmsToken.GetType().GetProperties().ToDictionary(x => x.Name.ToFirstLower(), x => x.GetValue(cmsToken)?.ToString());
        foreach (var contentBlock in contentBlockItems)
        {
            var template = new Template(contentBlock.Value).ReplaceDataTokens(cmsTokenValuesDict, new string[] { "text", "currency", "date" });
            result.Add(new ContentBlockItem(contentBlock.Key, contentBlock.Header, template));
        }

        return result;
    }

    public List<ContentBlockItem> ReplaceCmsTokens(IEnumerable<ContentBlockItem> contentBlockItems, Dictionary<string, object> props)
    {
        var result = new List<ContentBlockItem>();
        var cmsTokenValuesDict = props.ToDictionary(x => x.Key.ToFirstLower(), x => x.Value?.ToString());

        foreach (var contentBlock in contentBlockItems)
        {
            var template = new Template(contentBlock.Value).ReplaceDataTokens(cmsTokenValuesDict, new string[] { "text", "currency", "date" });
            result.Add(new ContentBlockItem(contentBlock.Key, contentBlock.Header, template));
        }

        return result;
    }

    public string GetOptionalPropertyValueAsString(JsonElement element, string propertyName)
    {
        var propertyExist = element.TryGetProperty(propertyName, out var property);
        if (!propertyExist)
            return string.Empty;

        return GetPropertyValue(element, propertyName).ToString();
    }

    public JsonElement TryGetPropertyValue(JsonElement element, string propertyName)
    {
        var summaryBlockProperty = GetPropertyByName(element, propertyName);
        if (!summaryBlockProperty.Value.TryGetProperty(ContentPropertyName.Values, out var values))
            return new JsonElement();

        return values;
    }

    protected virtual string GetTokenOrCalcApiValue(JsonElement element, Dictionary<string, object> props)
    {
        var valueToken = GetPropertyValue(element, ContentPropertyName.Value).ToString();
        var selectedQuoteApiValue = valueToken.Replace(".", "_");

        var isQuoteValue = props.TryGetValue(selectedQuoteApiValue, out var quotev2Value);
        if (!isQuoteValue)
            return valueToken;

        return quotev2Value.ToString();
    }

    private string GetExplanationSummaryItemValue(JsonElement element, Dictionary<string, object> props)
    {
        var valueToken = GetPropertyValue(element, ContentPropertyName.Value).ToString();
        var selectedQuoteApiValue = valueToken.Replace(".", "_");

        var isQuoteValue = props.TryGetValue(selectedQuoteApiValue, out var quotev2Value);
        if (!isQuoteValue)
            return string.Empty;

        return quotev2Value.ToString();
    }
    private async Task<BottomInformationItem> GetBottomInformation(JsonElement element, string propertyName, (string AccessToken, string Env, string Bgroup) auth)
    {
        var result = new List<BottomInformationValue>();
        var propertyItemExits = element.TryGetProperty(propertyName, out var propertyItem);
        if (!propertyItemExits)
            return new BottomInformationItem();

        var valuesPropertyExits = propertyItem.TryGetProperty(ContentPropertyName.Values, out var propertyItemValues);
        if (valuesPropertyExits && propertyItemValues.ValueKind == JsonValueKind.Array)
        {
            foreach (var val in propertyItemValues.EnumerateArray())
            {

                val.TryGetProperty(ContentPropertyName.Elements, out var elementProperty);
                var contentProperty = GetPropertyByName(elementProperty, ContentPropertyName.Content);
                var contentValue = GetPropertyByName(contentProperty.Value, ContentPropertyName.Value);

                var uris = _cmsDataParser.GetDataSummaryBlockSourceUris(val);
                if (auth == default || !uris.Any())
                {
                    result.Add(new BottomInformationValue(contentValue.Value.ToString()));
                    continue;
                }
                var props = await _mdpClient.GetData(uris, auth);

                var cmsTokenValuesDict = props.ToDictionary(x => x.Key.ToFirstLower(), x => DateTime.TryParse(x.Value?.ToString(), out var vel) ? vel.ToString("dd MMM yyyy") : x.Value?.ToString());
                var content = new Template(contentValue.Value.ToString()).ReplaceDataTokens(cmsTokenValuesDict, new string[] { "text", "currency", "date" });
                result.Add(new BottomInformationValue(content));
            }
        }
        return new BottomInformationItem { Values = result };
    }

    public JsonElement GetPropertyValue(JsonElement element, string propertyName)
    {
        var propertyItem = GetPropertyByName(element, propertyName);
        return GetPropertyByName(propertyItem.Value, ContentPropertyName.Value).Value;
    }

    private string GetDescriptionTokens(JsonElement element, string propertyName, Dictionary<string, object> props)
    {
        var propertyItem = GetPropertyByName(element, propertyName);
        var names = GetPropertyByName(propertyItem.Value, ContentPropertyName.Value);

        var cmsTokenValuesDict = props.ToDictionary(x => x.Key.ToFirstLower(), x => DateTime.TryParse(x.Value?.ToString(), out var vel) ? vel.ToString("dd MMM yyyy") : x.Value?.ToString());
        var content = new Template(names.Value.ToString()).ReplaceDataTokens(cmsTokenValuesDict, new string[] { "text", "currency", "date" });

        return content;
    }

    private JsonProperty GetPropertyByName(JsonElement element, string propertyName)
    {
        return element.EnumerateObject().FirstOrDefault(x => x.NameEquals(propertyName));
    }
}