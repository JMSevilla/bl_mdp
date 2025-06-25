using System.Collections.Generic;
using LanguageExt;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;
using WTW.MdpService.Infrastructure.Content;
using WTW.MdpService.Infrastructure.Templates.Common.CmsContentModels;
using WTW.MdpService.Retirement;

namespace WTW.MdpService.Journeys.Submit.Services;

public class TemplateDataDetails
{
    public TemplateResponse Template { get; set; }
    public Member Member { get; set; }
    public IEnumerable<SummaryBlock> SummaryBlocks { get; set; }
    public IEnumerable<ContentBlockItem> ContentBlocks { get; set; }
    public IEnumerable<DataSummaryItem> DataSummaries { get; set; }
    public Option<Calculation> Calculations { get; set; }
    public CmsTokenInformationResponse CmsTokens { get; set; }
}