namespace WTW.MdpService.Infrastructure.Calculations;

public class PartialTransferResponse
{
    public ErrorsResponse Errors { get; init; }
    public ResultsResponse Results { get; init; }

    public record ResultsResponse
    {
        public MdpResponse Mdp { get; init; }
    }

    public record MdpResponse
    {
        public PensionTranchesResidualResponse PensionTranchesResidual { get; init; }
        public PensionTranchesFull PensionTranchesFull { get; init; }
        public TransferValuesFullResponse TransferValuesFull { get; init; }
        public TransferValuesPartialResponse TransferValuesPartial { get; init; }
    }

    public record PensionTranchesResidualResponse
    {
        public decimal Total { get; init; }
        public decimal Gmp { get; init; }
        public decimal Pre97Excess { get; init; }
        public decimal Post97 { get; init; }
    }

    public record PensionTranchesFull
    {
        public decimal Pre88Gmp { get; init; }
        public decimal Post88Gmp { get; init; }
        public decimal Pre97Excess { get; init; }
        public decimal Post97 { get; init; }
        public decimal Total { get; init; }
    }

    public record TransferValuesFullResponse
    {
        public decimal Gmp { get; init; }
        public decimal Total { get; init; }
        public decimal NonGuaranteed { get; init; }
        public decimal Pre97Excess { get; init; }
        public decimal Post97 { get; init; }
    }

    public record TransferValuesPartialResponse
    {
        public decimal Total { get; init; }
        public decimal Gmp { get; init; }
        public decimal Pre97Excess { get; init; }
        public decimal Post97 { get; init; }
    }
}