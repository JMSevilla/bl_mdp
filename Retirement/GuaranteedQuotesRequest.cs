using System;
using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Retirement;

public class GuaranteedQuotesRequest
{
    public DateTimeOffset? GuaranteeDateFrom { get; set; } = null;

    public DateTimeOffset? GuaranteeDateTo { get; set; } = null;

    [EnumDataType(typeof(EventValue), ErrorMessage = "Invalid Quotation Status. It should be either 'retirement' or 'transfer'")]
    public string? Event { get; set; } = string.Empty;

    [EnumDataType(typeof(QuotationStatusValue), ErrorMessage = "Invalid Quotation Status.")]
    public string? QuotationStatus { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "PageNumber must be greater than or equal to 0.")]
    public int? PageNumber { get; set; } = 1;

    [Range(0, int.MaxValue, ErrorMessage = "PageSize must be greater than or equal to 0.")]
    public int? PageSize { get; set; } = 500;


    public enum QuotationStatusValue
    {
        GUARANTEED,
        ACCEPTED,
        CANCELLED,
        EXPIRED,
    }

    public enum EventValue
    {
        retirement,
        transfer,
    }
}
