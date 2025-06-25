using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.Templates.Common.CmsContentModels;

public class BottomInformationItem
{
    public List<BottomInformationValue> Values { get; set; } = new();
}