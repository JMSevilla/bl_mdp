using WTW.MdpService.Infrastructure.Calculations;

namespace WTW.MdpService.TransferJourneys;

public record TransferValueResponse
{
    public decimal PensionTranchesResidualTotal { get; init; }
    public decimal NonGuaranteed { get; init; }

    public static TransferValueResponse From(PartialTransferResponse.MdpResponse response)
    {
        return new()
        {
            PensionTranchesResidualTotal = response.PensionTranchesResidual.Total,
            NonGuaranteed = response.TransferValuesFull.NonGuaranteed,
        };
    }
}