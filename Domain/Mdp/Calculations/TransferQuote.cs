using System;
using System.Collections.Generic;
using WTW.MdpService.Infrastructure.Calculations;

namespace WTW.MdpService.Domain.Mdp.Calculations;

public class TransferQuote
{
    public TransferQuote(TransferResponse transferResponse)
    {
        GuaranteeDate = transferResponse.Results.Mdp.GuaranteeDate;
        GuaranteePeriod = transferResponse.Results.Mdp.GuaranteePeriod;
        ReplyByDate = transferResponse.Results.Mdp.ReplyByDate;
        TotalPensionAtDOL = transferResponse.Results.Mdp.TotalPensionAtDOL;
        MaximumResidualPension = transferResponse.Results.Mdp.MaximumResidualPension;
        MinimumResidualPension = transferResponse.Results.Mdp.MinimumResidualPension;
        IsGuaranteedQuote = transferResponse.Results.Mdp.IsGuaranteedQuote;
        TransferValues = new TransferValues(transferResponse.Results.Mdp.TransferValues);
        OriginalEffectiveDate = transferResponse.Results.Mdp.OriginalEffectiveDate;
        WordingFlags = transferResponse.Results.Mdp.WordingFlags;
    }

    public TransferQuote(TransferQuoteDto transferQuoteDto)
    {
        GuaranteeDate = transferQuoteDto.GuaranteeDate;
        GuaranteePeriod = transferQuoteDto.GuaranteePeriod;
        ReplyByDate = transferQuoteDto.ReplyByDate;
        TotalPensionAtDOL = transferQuoteDto.TotalPensionAtDOL;
        MaximumResidualPension = transferQuoteDto.MaximumResidualPension;
        MinimumResidualPension = transferQuoteDto.MinimumResidualPension;
        IsGuaranteedQuote = transferQuoteDto.IsGuaranteedQuote;
        TransferValues = new TransferValues(transferQuoteDto.TransferValues);
        OriginalEffectiveDate = transferQuoteDto.OriginalEffectiveDate;
        WordingFlags = transferQuoteDto.WordingFlags;
    }

    public DateTimeOffset? GuaranteeDate { get; init; }
    public string GuaranteePeriod { get; init; }
    public DateTimeOffset? ReplyByDate { get; init; }
    public decimal TotalPensionAtDOL { get; init; }
    public decimal MaximumResidualPension { get; init; }
    public decimal MinimumResidualPension { get; init; }
    public TransferValues TransferValues { get; init; }
    public bool IsGuaranteedQuote { get; init; }
    public DateTimeOffset? OriginalEffectiveDate { get; init; }
    public IList<string> WordingFlags { get; }
}

public class TransferValues
{
    public decimal TotalGuaranteedTransferValue { get; init; }
    public decimal TotalNonGuaranteedTransferValue { get; init; }
    public decimal MinimumPartialTransferValue { get; init; }
    public decimal MaximumPartialTransferValue { get; init; }
    public decimal TotalTransferValue { get; init; }

    public TransferValues(TransferValuesDto transferValuesDto)
    {
        TotalTransferValue = transferValuesDto.TotalTransferValue;
        MinimumPartialTransferValue = transferValuesDto.MinimumPartialTransferValue;
        MaximumPartialTransferValue = transferValuesDto.MaximumPartialTransferValue;
        TotalGuaranteedTransferValue = transferValuesDto.TotalGuaranteedTransferValue;
        TotalNonGuaranteedTransferValue = transferValuesDto.TotalNonGuaranteedTransferValue;
    }

    public TransferValues(TransferResponse.TransferValuesResponse valuesResponse)
    {
        TotalTransferValue = valuesResponse.TotalTransferValue;
        MinimumPartialTransferValue = valuesResponse.MinimumPartialTransferValue;
        MaximumPartialTransferValue = valuesResponse.MaximumPartialTransferValue;
        TotalGuaranteedTransferValue = valuesResponse.TotalGuaranteedTransferValue;
        TotalNonGuaranteedTransferValue = valuesResponse.TotalNonGuaranteedTransferValue;
    }

    public string Type()
    {
        return (TotalGuaranteedTransferValue, TotalNonGuaranteedTransferValue) switch
        {
            (decimal tgt, decimal tngt) when tgt != default && tngt == default => "Guaranteed",
            (decimal tgt, decimal tngt) when tgt == default && tngt != default => "NotGuaranteed",
            _ => "Both"
        };
    }
}