using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.Templates.Common.CmsContentModels;

public class OptionBlock
{
    public string Header { get; set; }
    public string Description { get; set; }
    public int? OrderNo { get; set; }
    public List<SummaryItem> SummaryItems { get; set; }
}