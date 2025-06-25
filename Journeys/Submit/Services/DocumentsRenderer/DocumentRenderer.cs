using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.MdpDb;
using WTW.MdpService.Infrastructure.MemberDb;
using WTW.MdpService.Infrastructure.PdfGenerator;
using WTW.MdpService.Infrastructure.Templates.Common;
using WTW.MdpService.Infrastructure.Templates.Common.CmsContentModels;
using WTW.MdpService.Infrastructure.Templates.GenericJourneys;
using WTW.MdpService.Journeys.Submit.Services.DocumentsRenderer;
using WTW.MdpService.Retirement;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Journeys.Submit.Services;

public class DocumentRenderer : IDocumentRenderer
{
    private readonly IContentClient _contentClient;
    private readonly IPdfGenerator _pdfGenerator;
    private readonly IMemberRepository _memberRepository;
    private readonly IGenericJourneysTemplate _genericJourneysTemplate;
    private readonly ILogger<DocumentRenderer> _logger;
    private readonly ICalculationsRepository _calculationsRepository;
    private readonly ICmsDataParser _cmsDataParser;
    private readonly ITemplateDataService _templateDataService;
    private readonly IJourneysRepository _journeysRepository;

    public DocumentRenderer(IContentClient contentClient,
        IPdfGenerator pdfGenerator,
        IMemberRepository memberRepository,
        IGenericJourneysTemplate genericJourneysTemplate,
        ILogger<DocumentRenderer> logger, 
        ICalculationsRepository calculationsRepository,
        ITemplateDataService templateDataService,
        ICmsDataParser cmsDataParser,
        IJourneysRepository journeysRepository)
    {
        _contentClient = contentClient;
        _pdfGenerator = pdfGenerator;
        _memberRepository = memberRepository;
        _genericJourneysTemplate = genericJourneysTemplate;
        _logger = logger;
        _calculationsRepository = calculationsRepository;
        _templateDataService = templateDataService;
        _cmsDataParser = cmsDataParser;
        _journeysRepository = journeysRepository;
    }

    public async Task<(string EmailHtmlBody, string EmailSubject, string EmailFrom, string EmailTo)> RenderGenericJourneySummaryEmail(
        DocumentsRendererData documentsRendererData, string accessToken, string env)
    {
        _logger.LogInformation("Rendering generic journey email. Case number: {caseNumber}.Journey type: {journeyType}. ", documentsRendererData.CaseNumber, documentsRendererData.JourneyType);
        var data = await GetTemplateNecessaryData(
            documentsRendererData.EmailTemplateKey,
            documentsRendererData.ReferenceNumber,
            documentsRendererData.BusinessGroup,
            documentsRendererData.AccessKey,
            documentsRendererData.DataSummaryBlockKey,
            accessToken,
            env);

        var journeyData = await GetAdditionalDataForEmail(documentsRendererData.BusinessGroup, documentsRendererData.ReferenceNumber, documentsRendererData.JourneyType);

        var renderedHtmlBody = await _genericJourneysTemplate.RenderHtml(
            data.Template.HtmlBody,
            journeyData,
            data.Member,
            DateTimeOffset.UtcNow,
            documentsRendererData.CaseNumber,
            data.SummaryBlocks,
            data.ContentBlocks);

        _logger.LogInformation("Completed Rendering generic journey email. Case number: {caseNumber}.Journey type: {journeyType}. ", documentsRendererData.CaseNumber, documentsRendererData.JourneyType);
        return (renderedHtmlBody, data.Template.EmailSubject, data.Template.EmailFrom, data.Member.Email().SingleOrDefault());
    }

    public async Task<(MemoryStream PdfStream, string FileName)> RenderGenericSummaryPdf(DocumentsRendererData documentsRendererData, string accessToken, string env)
    {
        _logger.LogInformation("Rendering generic journey summary pdf. Case number: {caseNumber}.Journey type: {journeyType}. ", documentsRendererData.CaseNumber, documentsRendererData.JourneyType);
        
        var data = await GetTemplateNecessaryData(
            documentsRendererData.PdfSummaryTemplateKey,
            documentsRendererData.ReferenceNumber,
            documentsRendererData.BusinessGroup,
            documentsRendererData.AccessKey,
            documentsRendererData.DataSummaryBlockKey,
            accessToken,
            env);
       
        var renderedHtmlBody = await _genericJourneysTemplate.RenderHtml(data.Template.HtmlBody, new
        {
            SystemDate = DateTimeOffset.UtcNow,
            SummaryBlocks = data.SummaryBlocks,
            ContentBlocks = data.ContentBlocks,
            DataSummaries = data.DataSummaries,
            MemberReferenceNumber = data.Member.ReferenceNumber,
            MemberTitle = data.Member.PersonalDetails.Title,
            MemberForenames = data.Member.PersonalDetails.Forenames,
            MemberSurname = data.Member.PersonalDetails.Surname,
            MemberDateOfBirth = data.Member.PersonalDetails.DateOfBirth?.ToString("dd MMMM yyyy"),
            CmsTokens = data.CmsTokens
        });

        var pdfStream = await _pdfGenerator.Generate(renderedHtmlBody, data.Template.HtmlHeader, data.Template.HtmlFooter);
        var fileName = ExtractFileNameFromHtmlMetadata(renderedHtmlBody)
                   ?? $"{documentsRendererData.PdfSummaryTemplateKey}.pdf";

        _logger.LogInformation("Completed rendering generic journey summary pdf. Case number: {caseNumber}.Journey type: {journeyType}. ", documentsRendererData.CaseNumber, documentsRendererData.JourneyType);
        return (pdfStream, fileName);
    }

    public async Task<(MemoryStream PdfStream, string FileName)> RenderDirectPdf(DocumentsRendererData documentsRendererData, string accessToken, string env)
    {
        _logger.LogInformation("Start rendering '{journeyType}' journey summary pdf. Pdf template key'{templateKey}'", documentsRendererData.JourneyType, documentsRendererData.PdfSummaryTemplateKey);

        var data = await GetTemplateNecessaryData(
            documentsRendererData.PdfSummaryTemplateKey,
            documentsRendererData.ReferenceNumber,
            documentsRendererData.BusinessGroup,
            documentsRendererData.AccessKey,
            documentsRendererData.DataSummaryBlockKey,
            accessToken,
            env);
        var member = data.Member;

        var renderedHtmlBody = documentsRendererData.PdfSummaryTemplateKey switch
        {
            "dc_pension_pot_summary_pdf" => await RenderDcPensionPotSummaryHtml(data.Template,
                                                                                data.Calculations.Value(),
                                                                                data.SummaryBlocks,
                                                                                data.ContentBlocks,
                                                                                data.CmsTokens,
                                                                                member,
                                                                                documentsRendererData.AccessKey,
                                                                                data.DataSummaries),
            "option_summary_pdf" => await RenderOptionSummaryHtml(data.Template,
                                                                  data.Calculations.Value(),
                                                                  data.SummaryBlocks,
                                                                  data.ContentBlocks,
                                                                  member,
                                                                  documentsRendererData.AccessKey,
                                                                  data.DataSummaries),
            _ => await _genericJourneysTemplate.RenderHtml(data.Template.HtmlBody, new
            {
                SystemDate = DateTimeOffset.UtcNow,
                SummaryBlocks = data.SummaryBlocks,
                ContentBlocks = data.ContentBlocks,
                DataSummaries = data.DataSummaries,
                MemberReferenceNumber = member.ReferenceNumber,
                MemberTitle = member.PersonalDetails.Title,
                MemberForenames = member.PersonalDetails.Forenames,
                MemberSurname = member.PersonalDetails.Surname,
                MemberDateOfBirth = member.PersonalDetails.DateOfBirth?.ToString("dd MMMM yyyy"),
            })
        };

        var pdfStream = await _pdfGenerator.Generate(renderedHtmlBody, data.Template.HtmlHeader, data.Template.HtmlFooter);
        var fileName = ExtractFileNameFromHtmlMetadata(renderedHtmlBody) ?? $"{documentsRendererData.PdfSummaryTemplateKey}.pdf";

        _logger.LogInformation("Completed rendering '{journeyType}' journey summary pdf. Pdf template key'{templateKey}'", documentsRendererData.JourneyType, documentsRendererData.PdfSummaryTemplateKey);

        return (pdfStream, fileName);
    }

    private async Task<TemplateDataDetails> GetTemplateNecessaryData(
        string templateName,
        string referenceNumber,
        string businessGroup,
        string accessKey,
        string dataSummaryBlockKey,
        string accessToken, 
        string env)
    {
        var memberTask = _memberRepository.FindMember(referenceNumber, businessGroup);
        var calculationOptionTask = _calculationsRepository.Find(referenceNumber, businessGroup);
        await Task.WhenAll(memberTask, calculationOptionTask);

        var member = (await memberTask).Value();
        var calculationOption = await calculationOptionTask;

        var template = await _contentClient.FindTemplate(templateName, accessKey, $"{member.SchemeCode}-{member.Category}");
        var cmsTokensTask = Task.Run(() => _templateDataService.GetCmsTokensResponseData(member, calculationOption));
        
        var dataSummaryKeys = _cmsDataParser.GetDataSummaryKeys(template);

        var genericDataSummaryBlocksTask = _templateDataService.GetGenericDataSummaryBlocks(
             dataSummaryBlockKey,
             accessKey,
             (accessToken, env, businessGroup));

        var genericDataSummariesBlocksTask = _templateDataService.GetGenericDataSummaryBlocks(
             dataSummaryKeys,
             accessKey,
             (accessToken, env, businessGroup));

        var contentBlockItemsTask = _templateDataService.GetGenericContentBlockItems(template, accessKey, (accessToken, env, businessGroup));

        await Task.WhenAll(genericDataSummaryBlocksTask, genericDataSummariesBlocksTask, contentBlockItemsTask, cmsTokensTask);

        var genericDataSummaryBlocks = await genericDataSummaryBlocksTask;
        var contentBlockItems = await contentBlockItemsTask;
        var cmsTokens = await cmsTokensTask;
        var genericDataSummariesBlocks = await genericDataSummariesBlocksTask;

        await Task.WhenAll(calculationOptionTask, cmsTokensTask, contentBlockItemsTask);

        return new TemplateDataDetails
        {
            Template = template,
            Member = member,
            SummaryBlocks = genericDataSummaryBlocks,
            ContentBlocks = contentBlockItems,
            DataSummaries = genericDataSummariesBlocks,
            Calculations = calculationOption,
            CmsTokens = cmsTokens
        };
    }

    private async Task<GenericJourney> GetAdditionalDataForEmail(string businessGroup, string referenceNumber, string journeyType)
    {
        var journey = (await _journeysRepository.Find(businessGroup, referenceNumber, journeyType)).Value();
        return journey;
    }

    private async Task<string> RenderOptionSummaryHtml(
        TemplateResponse template,
        Calculation calculation,
        IEnumerable<SummaryBlock> genericDataSummaryBlocks,
        IEnumerable<ContentBlockItem> contentBlockItems,
        Member member,
        string contentAccessKey,
        IEnumerable<DataSummaryItem> dataSummaries)
    {
        var optionSummaryDataSummaryBlocks = await _templateDataService.GetOptionSummaryDataSummaryBlocks(calculation, contentAccessKey);

        return await _genericJourneysTemplate.RenderHtml(template.HtmlBody, new
        {
            SystemDate = DateTimeOffset.UtcNow,
            SummaryBlocks = genericDataSummaryBlocks.Concat(optionSummaryDataSummaryBlocks),
            DataSummaries = dataSummaries,
            ContentBlocks = contentBlockItems,
            MemberReferenceNumber = member.ReferenceNumber,
            MemberTitle = member.PersonalDetails.Title,
            MemberForenames = member.PersonalDetails.Forenames,
            MemberSurname = member.PersonalDetails.Surname,
            MemberDateOfBirth = member.PersonalDetails.DateOfBirth?.ToString("dd MMMM yyyy"),
        });
    }

    private async Task<string> RenderDcPensionPotSummaryHtml(
        TemplateResponse template,
        Calculation calculation,
        IEnumerable<SummaryBlock> genericDataSummaryBlocks,
        IEnumerable<ContentBlockItem> contentBlockItems,
        CmsTokenInformationResponse cmsTokens,
        Member member,
        string contentAccessKey,
        IEnumerable<DataSummaryItem> dataSummaries)
    {
        var optionListItems = await _templateDataService.GetOptionListItems(calculation, contentAccessKey);
        return await _genericJourneysTemplate.RenderHtml(template.HtmlBody, new
        {
            SystemDate = DateTimeOffset.UtcNow,
            SummaryBlocks = genericDataSummaryBlocks,
            DataSummaries = dataSummaries,
            OptionList = optionListItems,
            MemberQuotePensionOptionNumber = 0,
            CmsTokens = cmsTokens,
            ContentBlocks = contentBlockItems,
            MemberReferenceNumber = member.ReferenceNumber,
            MemberTitle = member.PersonalDetails.Title,
            MemberForenames = member.PersonalDetails.Forenames,
            MemberSurname = member.PersonalDetails.Surname,
            MemberDateOfBirth = member.PersonalDetails.DateOfBirth?.ToString("dd MMMM yyyy"),
        });
    }

    private static string ExtractFileNameFromHtmlMetadata(string html)
    {
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);
        
        return htmlDocument.DocumentNode
            .SelectSingleNode("//meta[@name='filename']")
            ?.GetAttributeValue("content", null);
    }
}