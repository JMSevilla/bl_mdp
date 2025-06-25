using System;
using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Calculations;

public class QuotationResponse
{
    [JsonPropertyName("guaranteed")]
    public bool Guaranteed { get; set; }
    [JsonPropertyName("expiryDate")]
    public DateTime? ExpiryDate { get; set; }
}