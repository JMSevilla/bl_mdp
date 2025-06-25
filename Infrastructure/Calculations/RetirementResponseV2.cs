namespace WTW.MdpService.Infrastructure.Calculations;

public record RetirementResponseV2
{
    public InputsResponse Inputs { get; init; }
    public ErrorsResponse Errors { get; init; }
    public ResultsResponse Results { get; init; }
}