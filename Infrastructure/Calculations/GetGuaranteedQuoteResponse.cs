using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WTW.MdpService.Infrastructure.Calculations;

public class GetGuaranteedQuoteResponse
{
    public GetGuaranteedQuoteResponse()
    {
        Pagination = new Pagination();
        Quotations = new List<Quotation>();
    }
    public Pagination Pagination { get; set; }
    public List<Quotation> Quotations { get; set; }
}

public class ApiResponse
{
    public int Test { get; set; }
}

public class Pagination
{
    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; set; }
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }
    [JsonPropertyName("totalRecords")]
    public int TotalRecords { get; set; }
}

public class Quotation
{
    [JsonPropertyName("runDate")]
    public DateTime? RunDate { get; set; }
    [JsonPropertyName("effectiveDate")]
    public DateTime? EffectiveDate { get; set; }
    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;
    [JsonPropertyName("calcType")]
    public string CalcType { get; set; } = string.Empty;
    [JsonPropertyName("imageId")]
    public int? ImageId { get; set; }
    [JsonPropertyName("calcSource")]
    public string CalcSource { get; set; } = string.Empty;
    [JsonPropertyName("apiResponse")]
    public ApiResponse ApiResponse { get; set; }
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    [JsonPropertyName("expiryDate")]
    public DateTime? ExpiryDate { get; set; }
}
