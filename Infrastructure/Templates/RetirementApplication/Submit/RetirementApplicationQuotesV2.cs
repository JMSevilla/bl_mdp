using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WTW.MdpService.Domain.Mdp;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.MdpApi;
using WTW.MdpService.Infrastructure.Templates.Common;
using WTW.MdpService.Journeys.Submit.Services.DocumentsRenderer;
using WTW.MdpService.Retirement;

namespace WTW.MdpService.Infrastructure.Templates.RetirementApplication.Submit;

public class RetirementApplicationQuotesV2 : CmsTemplateContentJsonParser, IRetirementApplicationQuotesV2
{
    private readonly ICalculationsParser _calculationsParser;
    private readonly IRetirementService _retirementService;
    private readonly IContentClient _contentClient;

    public RetirementApplicationQuotesV2(
        ICalculationsParser calculationsParser,
        IRetirementService retirementService,
        IContentClient contentClient,
        IMdpClient mdpClient,
        ICmsDataParser cmsDataParser) : base(mdpClient,cmsDataParser)
    {
        _calculationsParser = calculationsParser;
        _retirementService = retirementService;
        _contentClient = contentClient;
    }

    public async Task<RetirementApplicationSubmitionTemplateData> Create(RetirementJourney journey, IEnumerable<string> contentKeys, string contentAccessKey, CmsTokenInformationResponse cmsToken)
    {
        if (journey.MemberQuote.PensionOptionNumber != 0)
            return new RetirementApplicationSubmitionTemplateData();

        var summary = await _contentClient.FindSummaryBlocks(journey.MemberQuote.Label, contentAccessKey);
        var retirementV2 = _calculationsParser.GetRetirementV2(journey.Calculation.RetirementJsonV2);
        var optionsDictionary = _retirementService.GetSelectedQuoteDetails(journey.MemberQuote.Label, retirementV2);
        var summaryBlocks = await GetSummaryBlocks(summary, optionsDictionary);

        var contentBlock = await _contentClient.FindContentBlocks(contentKeys, contentAccessKey);
        var contentBLocks = GetContentBlock(contentBlock).ToList();
        var replacedContentBlockTokens = ReplaceCmsTokens(contentBLocks, cmsToken);

        return new RetirementApplicationSubmitionTemplateData
        {
            SelectedOptionData = optionsDictionary,
            SummaryBlocks = summaryBlocks,
            ContentBlockItems = replacedContentBlockTokens
        };
    }

    public async Task<RetirementSummary> GetSummaryFigures(RetirementJourney journey, JsonElement retirementOptions)
    {
        var result = new RetirementSummary();

        if (journey.MemberQuote.PensionOptionNumber != 0)
            return result;

        var rootQuote = journey.MemberQuote.Label.Split(".").FirstOrDefault();

        var quotes = retirementOptions
            .EnumerateArray()
            .Where(x =>
                     JsonSerializer.Serialize(x).Contains(rootQuote + ".totalPension") ||
                     JsonSerializer.Serialize(x).Contains(rootQuote + ".totalLumpSum"))
            .FirstOrDefault();

        var retirementV2 = _calculationsParser.GetRetirementV2(journey.Calculation.RetirementJsonV2);
        var optionsDictionary = _retirementService.GetSelectedQuoteDetails(journey.MemberQuote.Label, retirementV2);

        quotes.TryGetProperty("elements", out var elementProperty);
        var summaryItems = GetSummaryItems(elementProperty, optionsDictionary);

        foreach (var item in summaryItems)
            result.SummaryFigures.Add(new SummaryFigure { Label = item.Header, Value = item.Value, Description = item.Description });

        return result;
    }
}