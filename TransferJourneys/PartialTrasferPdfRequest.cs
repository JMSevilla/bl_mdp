using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.TransferJourneys;

public record PartialTrasferPdfRequest
{
    [Required]
    public string ContentAccessKey { get; init; }
    public decimal? RequestedTransferValue { get; init; }
    public decimal? RequestedResidualPension { get; init; }
}