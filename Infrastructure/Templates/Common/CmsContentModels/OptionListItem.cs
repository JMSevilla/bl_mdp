using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.Templates.Common.CmsContentModels;

public record OptionListItem
{
    public IDictionary<string, object> Quotev2 { get; init; }
    public string Header { get; init; }
    public string Description  { get; init; }
    public int? OptionNumber { get; init; }
    public IEnumerable<SummaryItem> SummaryItems { get; init; }
}