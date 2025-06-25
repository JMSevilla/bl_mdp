using System;

namespace WTW.MdpService.Infrastructure.Calculations;

public class GetGuaranteedQuoteClientRequest
{
    public string Bgroup { get; set; } = string.Empty;
    public string RefNo { get; set; } = string.Empty;
    public DateTimeOffset? GuaranteeDateFrom { get; set; }
    public DateTimeOffset? GuaranteeDateTo { get; set; }
    public string Event { get; set; } = string.Empty;
    public string QuotationStatus { get; set; } = string.Empty;
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 500;
}