using System.Collections.Generic;
using WTW.MdpService.Infrastructure.Templates.Common.CmsContentModels;

namespace WTW.MdpService.Infrastructure.Templates.RetirementApplication.Submit;

public class RetirementApplicationSubmitionTemplateData
{
    public Dictionary<string, object> SelectedOptionData { get; set; } = new();

    public List<SummaryBlock> SummaryBlocks { get; set; } = new();

    public List<ContentBlockItem> ContentBlockItems { get; set; } = new();
}