using WTW.MdpService.Infrastructure.Calculations;

namespace WTW.MdpService.TransferJourneys;

public record PensionIncomeResponse
{
    public decimal TransferValuesFullTotal { get; init; }
    public decimal NonGuaranteed { get; init; }
    public decimal TransferValuesPartialTotal { get; init; }

    public static PensionIncomeResponse From(PartialTransferResponse.MdpResponse response)
    {
        return new()
        {
            TransferValuesFullTotal = response.TransferValuesFull.Total,
            NonGuaranteed = response.TransferValuesFull.NonGuaranteed,
            TransferValuesPartialTotal = response.TransferValuesPartial.Total
        };
    }
}