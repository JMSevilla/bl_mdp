using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.MdpApi;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.Templates.Common.CmsContentModels;
using WTW.MdpService.Infrastructure.Templates.GenericJourneys;
using WTW.MdpService.Infrastructure.Templates.RetirementApplication.Calculations;
using WTW.MdpService.Journeys.Submit.Services.DocumentsRenderer;
using WTW.MdpService.Retirement;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Infrastructure.Templates.Common;

public class TemplateDataService : ITemplateDataService
{
    private readonly IGenericTemplateContent _genericTemplateContent;
    private readonly ICmsDataParser _cmsDataParser;
    private readonly IContentClient _contentClient;
    private readonly IMdpClient _mdpClient;
    private readonly ICalculationsParser _calculationsParser;
    private readonly IRetirementCalculationQuotesV2 _retirementCalculationQuotes;
    private readonly IQuoteSelectionJourneyRepository _quoteSelectionJourneyRepository;
    private readonly ILogger<TemplateDataService> _logger;

    public TemplateDataService(IGenericTemplateContent genericTemplateContent,
        ICmsDataParser cmsDataParser,
        IContentClient contentClient,
        IMdpClient mdpClient,
        ICalculationsParser calculationsParser,
        IRetirementCalculationQuotesV2 retirementCalculationQuotes,
        IQuoteSelectionJourneyRepository quoteSelectionJourneyRepository,
        ILogger<TemplateDataService> logger)
    {
        _genericTemplateContent = genericTemplateContent;
        _cmsDataParser = cmsDataParser;
        _contentClient = contentClient;
        _mdpClient = mdpClient;
        _calculationsParser = calculationsParser;
        _retirementCalculationQuotes = retirementCalculationQuotes;
        _quoteSelectionJourneyRepository = quoteSelectionJourneyRepository;
        _logger = logger;
    }

    public CmsTokenInformationResponse GetCmsTokensResponseData(Member member, Option<Calculation> calculationOption)
    {
        var builder = new CmsTokenInformationResponseBuilder()
            .CalculationSuccessful(true);

        return calculationOption
            .Match<CmsTokenInformationResponse>(c =>
            {
                return builder.WithRetirementV2Data(
                    string.IsNullOrWhiteSpace(c.RetirementJsonV2) ? null : _calculationsParser.GetRetirementV2(c.RetirementJsonV2),
                    c.RetirementJourney?.MemberQuote.Label,
                    member.BusinessGroup,
                    member.Scheme?.Type).Build();
            },
        () =>
        {
            _logger.LogInformation("building Cms Information tokens with no calculation object.");
            return builder.WithRetirementV2Data(null, null, member.BusinessGroup, member.Scheme?.Type).Build();
        });
    }

    public async Task<IEnumerable<ContentBlockItem>> GetGenericContentBlockItems(TemplateResponse template, string contentAccessKey, (string AccessToken, string Env, string Bgroup) auth)
    {
        var contentKeys = _cmsDataParser.GetContentBlockKeys(template);
        var contentBlocks = await _contentClient.FindContentBlocks(contentKeys, contentAccessKey);
        if (contentBlocks == null)
        {
            _logger.LogWarning("Content blocks with keys '{contentBlockKeys}' not found. Access Key: '{accessKey}'.", string.Join(";", contentKeys), contentAccessKey);
            return Enumerable.Empty<ContentBlockItem>();
        }

        var uris = _cmsDataParser.GetContentBlockSourceUris(contentBlocks);
        var data = await _mdpClient.GetData(uris, auth);

        return _genericTemplateContent.GetContentBlockItems(contentBlocks, data, auth);
    }

    public async Task<IEnumerable<SummaryBlock>> GetGenericDataSummaryBlocks(string dataSummaryBlockKey, string contentAccessKey, (string AccessToken, string Env, string Bgroup) auth)
    {
        var dataSummaryContentOption = await _contentClient.FindDataSummaryBlocks(dataSummaryBlockKey, contentAccessKey);
        if (dataSummaryContentOption.IsNone)
        {
            _logger.LogWarning("Data summary blocks with key '{dataSummaryBlockKey}' not found. Access Key: '{accessKey}'.", dataSummaryBlockKey, contentAccessKey);
            return Enumerable.Empty<SummaryBlock>();
        }

        var uris = _cmsDataParser.GetDataSummaryBlockSourceUris(dataSummaryContentOption.Value());
        var data = await _mdpClient.GetData(uris, auth);
        return await _genericTemplateContent.GetDataSummaryBlocks(dataSummaryContentOption.Value(), data, auth);
    }

    public async Task<IEnumerable<DataSummaryItem>> GetGenericDataSummaryBlocks(IEnumerable<string> dataSummaryBlockKeys, string contentAccessKey, (string AccessToken, string Env, string Bgroup) auth)
    {
        var dataSummariesContent = await _contentClient.FindDataSummaryBlocks(dataSummaryBlockKeys, contentAccessKey);
        if (dataSummariesContent == null)
        {
            _logger.LogWarning("Data summary blocks with keys '{dataSummaryBlockKeys}' not found. Access Key: '{accessKey}'.", string.Join(";", dataSummaryBlockKeys), contentAccessKey);
            return Enumerable.Empty<DataSummaryItem>();
        }

        var uris = _cmsDataParser.GetContentBlockSourceUris(dataSummariesContent.Select(x => x.DataSummaryJsonElement));
        var data = await _mdpClient.GetData(uris, auth);
        return await _genericTemplateContent.GetDataSummaryBlocks(dataSummariesContent, data, auth);
    }

    public async Task<IOrderedEnumerable<OptionListItem>> GetOptionListItems(Calculation calculation, string accessKey)
    {
        _logger.LogInformation("Started building Option List items.");
        var optionsContent = await _contentClient.FindRetirementOptions(accessKey);
        var quotes = _calculationsParser.GetQuotesV2(calculation.QuotesJsonV2);

        var optionList = new List<OptionListItem>();
        foreach (var property in quotes.Options.EnumerateObject())
        {
            var (quotesV2, _) = await _retirementCalculationQuotes.Create(calculation, property.Name, new JsonElement());
            var optionsBlock = _retirementCalculationQuotes.FilterOptionsByKey(optionsContent, calculation, property.Name);
            if(optionsBlock.IsNone)
                continue;
            int? optionNumber = optionsBlock.Value().OrderNo;
            if (TryExtractOptionNumber(property.Value, out int? optionNumberElement))
                optionNumber = optionNumberElement;

            var summaryObject = new OptionListItem
            {
                Quotev2 = quotesV2,
                Header = optionsBlock.Value().Header,
                Description = optionsBlock.Value().Description,
                OptionNumber = optionNumber,
                SummaryItems = optionsBlock.Value().SummaryItems
            };
            optionList.Add(summaryObject);
        }

        _logger.LogInformation("Finished building Option List items.");
        return optionList.OrderBy(item => ((dynamic)item).OptionNumber);
    }

    public async Task<IEnumerable<SummaryBlock>> GetOptionSummaryDataSummaryBlocks(Calculation calculation, string contentAccessKey)
    {
        return await _quoteSelectionJourneyRepository.Find(calculation.BusinessGroup, calculation.ReferenceNumber)
            .ToAsync()
            .MatchAsync<IEnumerable<SummaryBlock>>(async j =>
            {              
                var summaryBlocksContent = await _contentClient.FindSummaryBlocks(j.QuoteSelection().Value(), contentAccessKey);

                return (await _retirementCalculationQuotes.Create(calculation, j.QuoteSelection().Value(), summaryBlocksContent)).Item2;
            }, () =>
            {
                _logger.LogWarning("No OptionSummary DataSummary blocks found. In Order failed to retrieve quote selection journey.");
                return Enumerable.Empty<SummaryBlock>();
            });
    }

    private bool TryExtractOptionNumber(JsonElement element, out int? optionNumber)
    {
        optionNumber = null;
        if (element.TryGetProperty("attributes", out var attributesElement) &&
            attributesElement.TryGetProperty("optionNumber", out var optionNumberElement) &&
            optionNumberElement.ValueKind == JsonValueKind.Number)
        {
            optionNumber = optionNumberElement.GetInt32();
            return true;
        }
        return false;
    }
}
