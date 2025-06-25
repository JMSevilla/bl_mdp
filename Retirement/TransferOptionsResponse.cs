using WTW.MdpService.Domain.Mdp.Calculations;
using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.Retirement;

public class TransferOptionsResponse
{
    private TransferOptionsResponse()
    {
    }

    public TransferOptionsResponse(TransferQuote quote, TransferApplicationStatus transferApplicationStatus)
    {
        IsCalculationSuccessful = true;
        TransferOption = transferApplicationStatus != TransferApplicationStatus.UnavailableTA
                ? transferApplicationStatus != TransferApplicationStatus.SubmitStarted 
                    ? $"transferFull{transferApplicationStatus}{quote.TransferValues.Type()}" 
                    : $"transferFull{TransferApplicationStatus.StartedTA.ToString()}{quote.TransferValues.Type()}"
                : "transferFullUnavailableTA";
        TotalGuaranteedTransferValue = quote.TransferValues.TotalGuaranteedTransferValue;
        TotalNonGuaranteedTransferValue = quote.TransferValues.TotalNonGuaranteedTransferValue;
        TotalTransferValue = quote.TransferValues.TotalTransferValue;
        TotalPensionAtDOL = quote.TotalPensionAtDOL;
        MinimumPartialTransferValue = quote.TransferValues.MinimumPartialTransferValue;
        MaximumPartialTransferValue = quote.TransferValues.MaximumPartialTransferValue == 0
            ? quote.TransferValues.TotalGuaranteedTransferValue
            : quote.TransferValues.MaximumPartialTransferValue;
        MinimumResidualPension = quote.MinimumResidualPension;
        MaximumResidualPension = quote.MaximumResidualPension;
    }
    public bool IsCalculationSuccessful { get; init; }
    public string TransferOption { get; init; }
    public decimal TotalGuaranteedTransferValue { get; init; }
    public decimal TotalNonGuaranteedTransferValue { get; init; }
    public decimal TotalTransferValue { get; init; }
    public decimal MinimumPartialTransferValue { get; init; }
    public decimal MaximumPartialTransferValue { get; init; }
    public decimal TotalPensionAtDOL { get; init; }
    public decimal MaximumResidualPension { get; init; }
    public decimal MinimumResidualPension { get; init; }

    public static TransferOptionsResponse CalculationFailed()
    {
        return new()
        {
            TransferOption = "transferFullUnavailableTA",
            IsCalculationSuccessful = false
        };
    }
}