using System;
using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Calculations;

public record RateOfReturn
{
    [JsonPropertyName("personalRateOfReturn")]
    public decimal? PersonalRateOfReturn { get; set; }

    [JsonPropertyName("openingBalance")]
    public decimal? OpeningBalance { get; set; }

    [JsonPropertyName("closingBalance")]
    public decimal? ClosingBalance { get; set; }

    [JsonPropertyName("totalContributions")]
    public decimal? TotalContributions { get; set; }

    [JsonPropertyName("totalDeductions")]
    public decimal? TotalDeductions { get; set; }

    [JsonPropertyName("changeInValue")]
    public decimal? ChangeInValue { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime? EndDate { get; set; }

    [JsonPropertyName("subTotal")]
    public decimal? SubTotal { get; set; }

    [JsonPropertyName("correctedStartDate")]
    public DateTime? CorrectedStartDate { get; set; }

    [JsonPropertyName("closingBalanceZero")]
    public string? ClosingBalanceZero { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
}
