using WTW.MdpService.Infrastructure.Calculations;

namespace WTW.MdpService.TransferJourneys;

public record TransferValueResponseV2
{
    public TransferValueResponseV2(PartialTransferResponse.MdpResponse response)
    {
        PensionTranchesResidualTotal = response.PensionTranchesResidual.Total;
        NonGuaranteed = response.TransferValuesFull.NonGuaranteed;
    }

    public decimal PensionTranchesResidualTotal { get; init; }
    public decimal NonGuaranteed { get; init; }
}