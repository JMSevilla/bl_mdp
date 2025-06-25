using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.JobScheduler;

public record OrderStatusResponse
{
    [JsonPropertyName("deliveryDate")]
    public DateTime DeliveryDate { get; set; }

    [JsonPropertyName("history")]
    public List<History> History { get; set; }
}

public record History
{
    [JsonPropertyName("endTime")]
    public DateTime EndTime { get; set; }

    [JsonPropertyName("historyId")]
    public string HistoryId { get; set; }

    [JsonPropertyName("jobChain")]
    public string JobChain { get; set; }

    [JsonPropertyName("jobschedulerId")]
    public string JobschedulerId { get; set; }

    [JsonPropertyName("node")]
    public string Node { get; set; }

    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }

    [JsonPropertyName("path")]
    public string Path { get; set; }

    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }

    [JsonPropertyName("state")]
    public State State { get; set; }

    [JsonPropertyName("surveyDate")]
    public DateTime SurveyDate { get; set; }
}

public record State
{
    [JsonPropertyName("_text")]
    public string Text { get; set; }

    [JsonPropertyName("severity")]
    public int Severity { get; set; }
}