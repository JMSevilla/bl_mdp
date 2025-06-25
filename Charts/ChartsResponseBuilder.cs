using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WTW.MdpService.Domain.Investment;

namespace WTW.MdpService.Charts;

public class ChartsResponseBuilder
{
    private readonly ChartsResponse _response;

    public ChartsResponseBuilder()
    {
        _response = new ChartsResponse();
    }

    public ChartsResponseBuilder WithData(IEnumerable<InvestmentFundChartData> chartData)
    {
        _response.Data = chartData;
        return this;
    }

    public ChartsResponseBuilder WithCurrency(string currency)
    {
        _response.Chart = new ChartDetailsResponse { Currency = currency };
        return this;
    }

    public ChartsResponseBuilder WithDataSetPortfolio()
    {
        _response.Datasets = new List<ChartDataSetResponse>
        {
            new ()
            {
                Name = "Portfolio",
                Data = new List<ChartDataResponse>
                {
                    new () { Label = "2023-10-15", Value = 5.10M },
                    new () { Label = "2023-10-16", Value = 4.95M },
                    new () { Label = "2023-10-17", Value = 5.30M },
                    new () { Label = "2023-10-18", Value = 5.23M },
                    new () { Label = "2023-10-19", Value = 4.89M },
                    new () { Label = "2023-10-20", Value = 6.12M }
                }
            }
        };

        return this;
    }

    public ChartsResponse Build()
    {
        return _response;
    }
}