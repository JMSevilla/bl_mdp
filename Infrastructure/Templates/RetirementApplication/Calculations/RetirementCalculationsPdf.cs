using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Calculations;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.PdfGenerator;
using WTW.MdpService.Retirement;

namespace WTW.MdpService.Infrastructure.Templates.RetirementApplication.Calculations;

public class RetirementCalculationsPdf : IRetirementCalculationsPdf
{
    private readonly IContentClient _contentClient;
    private readonly IPdfGenerator _pdfGenerator;
    private readonly ICalculationsParser _calculationsParser;
    private readonly IRetirementApplicationCalculationTemplate _retirementApplicationCalculationTemplate;

    public RetirementCalculationsPdf(
        IContentClient contentClient,
        IPdfGenerator pdfGenerator,
        ICalculationsParser calculationsParser,
        IRetirementApplicationCalculationTemplate retirementApplicationCalculationTemplate)
    {
        _contentClient = contentClient;
        _pdfGenerator = pdfGenerator;
        _calculationsParser = calculationsParser;
        _retirementApplicationCalculationTemplate = retirementApplicationCalculationTemplate;
    }

    public async Task<MemoryStream> GenerateSummaryPdf(
       string contentAccessKey,
       Calculation calculation,
       Member member,
       string summaryKey,
       string businessGroup,
       string accessToken,
       string env)
    {
        var template = await _contentClient.FindTemplate("option_summary_pdf", contentAccessKey, $"{member.SchemeCode}-{member.Category}");
        var summaryBlocks = await _contentClient.FindSummaryBlocks(summaryKey, contentAccessKey);
        var contentKeys = !string.IsNullOrEmpty(template.ContentBlockKeys) ? template.ContentBlockKeys.Split(";").Where(x => !string.IsNullOrEmpty(x)).ToList() : new List<string>();
        var contentBlocks = await _contentClient.FindContentBlocks(contentKeys, contentAccessKey);
        var cmsTokens = new CmsTokenInformationResponseBuilder()
        .CalculationSuccessful(true)
        .WithRetirementV2Data(string.IsNullOrWhiteSpace(calculation.RetirementJsonV2) ? null : _calculationsParser.GetRetirementV2(calculation.RetirementJsonV2),
                              calculation.RetirementJourney?.MemberQuote.Label, businessGroup, member.Scheme?.Type)
            .Build();
        var htmlBody = await _retirementApplicationCalculationTemplate.Render(template.HtmlBody, summaryKey, summaryBlocks, cmsTokens, calculation, member, contentBlocks, (accessToken, env, businessGroup));
        return await _pdfGenerator.Generate(htmlBody, template.HtmlHeader, template.HtmlFooter);
    }

    public async Task<MemoryStream> GenerateOptionsPdf(
        string contentAccessKey,
        Calculation calculation,
        Member member,
        string businessGroup,
        string accessToken,
        string env)
    {
        var template = await _contentClient.FindTemplate("dc_list_of_retirement_options", contentAccessKey, $"{member.SchemeCode}-{member.Category}");
        var optionsJson = await _contentClient.FindRetirementOptions(contentAccessKey);
        var quotesV2 = _calculationsParser.GetQuotesV2(calculation.QuotesJsonV2);

        var contentKeys = !string.IsNullOrEmpty(template.ContentBlockKeys) ? template.ContentBlockKeys.Split(";").Where(x => !string.IsNullOrEmpty(x)).ToList() : new List<string>();
        var contentBlocks = await _contentClient.FindContentBlocks(contentKeys, contentAccessKey);

        var cmsTokens = new CmsTokenInformationResponseBuilder()
            .CalculationSuccessful(true)
            .WithRetirementV2Data(string.IsNullOrWhiteSpace(calculation.RetirementJsonV2) ? null : _calculationsParser.GetRetirementV2(calculation.RetirementJsonV2),
                                  calculation.RetirementJourney?.MemberQuote.Label, businessGroup, member.Scheme?.Type)
            .Build();
        var htmlBody = await _retirementApplicationCalculationTemplate.Render(template.HtmlBody, optionsJson, quotesV2.Options, cmsTokens, calculation, contentBlocks, (accessToken, env, businessGroup));

        return await _pdfGenerator.Generate(htmlBody, template.HtmlHeader, template.HtmlFooter);
    }
}
