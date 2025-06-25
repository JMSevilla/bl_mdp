using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.Templates.Common.CmsContentModels;

public record SummaryBlock(string Header, BottomInformationItem BottomInformation)
{
    public List<SummaryItem> SummaryItems { get; set; } = new();
}