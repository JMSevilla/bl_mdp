using System;
using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.Calculations;

public record TransferQuoteDto
{
    public DateTimeOffset? GuaranteeDate { get; set; }
    public string GuaranteePeriod { get; set; }
    public DateTimeOffset? ReplyByDate { get; set; }
    public decimal TotalPensionAtDOL { get; set; }
    public decimal MaximumResidualPension { get; set; }
    public decimal MinimumResidualPension { get; set; }
    public TransferValuesDto TransferValues { get; set; }
    public bool IsGuaranteedQuote { get; set; }
    public DateTimeOffset? OriginalEffectiveDate { get; set; }
    public IList<string> WordingFlags { get; set; }
}

public record TransferValuesDto
{
    public decimal TotalGuaranteedTransferValue { get; set; }
    public decimal TotalNonGuaranteedTransferValue { get; set; }
    public decimal MinimumPartialTransferValue { get; set; }
    public decimal MaximumPartialTransferValue { get; set; }
    public decimal TotalTransferValue { get; set; }  
}