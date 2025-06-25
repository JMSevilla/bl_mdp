using System;
using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.Calculations;

public record TransferResponse
{
    public ErrorsResponse Errors { get; init; }
    public ResultsResponse Results { get; init; }

    public record ResultsResponse
    {
        public MdpResponse Mdp { get; init; }
    }

    public record MdpResponse
    {
        public DateTimeOffset? GuaranteeDate { get; init; }
        public string GuaranteePeriod { get; init; }
        public DateTimeOffset? ReplyByDate { get; init; }
        public decimal TotalPensionAtDOL { get; init; }
        public decimal MaximumResidualPension { get; init; }
        public decimal MinimumResidualPension { get; init; }
        public TransferValuesResponse TransferValues { get; init; }
        public bool IsGuaranteedQuote { get; init; }
        public DateTimeOffset? OriginalEffectiveDate { get; set; }
        public IList<string> WordingFlags { get; init; }
    }

    public record TransferValuesResponse
    {
        public decimal TotalGuaranteedTransferValue { get; set; }
        public decimal TotalNonGuaranteedTransferValue { get; set; }
        public decimal MinimumPartialTransferValue { get; init; }
        public decimal MaximumPartialTransferValue { get; init; }
        public decimal TotalTransferValue { get; init; }
    }
}