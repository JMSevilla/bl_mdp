using WTW.MdpService.Infrastructure.Calculations;

namespace WTW.MdpService.TransferJourneys;

public record PensionIncomeResponseV2
{
    public PensionIncomeResponseV2(PartialTransferResponse.MdpResponse response)
    {
        TransferValuesFullTotal = response.TransferValuesFull.Total;
        NonGuaranteed = response.TransferValuesFull.NonGuaranteed;
        TransferValuesPartialTotal = response.TransferValuesPartial.Total;
    }

    public decimal TransferValuesFullTotal { get; init; }
    public decimal NonGuaranteed { get; init; }
    public decimal TransferValuesPartialTotal { get; init; }
}