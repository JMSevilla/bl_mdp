using System;

namespace WTW.MdpService.Infrastructure.Templates.TransferApplication;

public class TransferApplicationTemplateDetails
{
    public string MemberQuoteReferenceNumber { get; set; }
    public DateTimeOffset? PtGeneratedDate { get; set; }
    public decimal? FpPre88Gmp { get; set; }
    public decimal? FpPost88Gmp { get; set; }
    public decimal? FpPre97Excess { get; set; }
    public decimal? FpPost97 { get; set; }
    public decimal? FpTotalPension { get; set; }
    public decimal? FtTransferGmp { get; set; }
    public decimal? FtPre97Excess { get; set; }
    public decimal? FtPost97 { get; set; }
    public decimal? FtTotalTransfer { get; set; }
    public decimal? PtTransferGmp { get; set; }
    public decimal? PtPre97Excess { get; set; }
    public decimal? PtPost97 { get; set; }
    public decimal? PtTotalTransfer { get; set; }
    public decimal? RpDeferredPension { get; set; }
    public decimal? RpPre97Excess { get; set; }
    public decimal? RpPost97 { get; set; }
    public decimal? RpTotalPension { get; set; }
}

