using System.Text.Json;

namespace WTW.MdpService.Infrastructure.Content;

public class DataSummaryBlock
{
    public string Key { get; set; }

    public JsonElement DataSummaryJsonElement { get; set; }
}
