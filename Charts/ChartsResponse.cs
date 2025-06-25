using System.Collections.Generic;
using WTW.MdpService.Domain.Investment;

namespace WTW.MdpService.Charts;

public class ChartsResponse
{
    public ChartDetailsResponse Chart { get; set; }
    public IEnumerable<InvestmentFundChartData> Data { get; set; }
    public IEnumerable<ChartDataSetResponse> Datasets { get; set; }
}