using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.Templates.Common.CmsContentModels;

public class DataSummaryItem
{
    public string Key { get; set; }
    public IEnumerable<SummaryBlock> SummaryBlocks { get; set; }
}
