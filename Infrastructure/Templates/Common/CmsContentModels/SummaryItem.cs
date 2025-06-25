using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.Templates.Common.CmsContentModels;

public record SummaryItem(string Header, string Format, string Divider, string Description, string Value)
{
    public List<ExplanationSummaryItem> ExplanationSummaryItems { get; set; } = new();
}
