using System.Collections.Generic;

namespace WTW.MdpService.Charts;

public class ChartDataSetResponse
{
    public string Name { get; set; }
    public List<ChartDataResponse> Data { get; set; }
}
