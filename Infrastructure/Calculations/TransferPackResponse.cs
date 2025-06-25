namespace WTW.MdpService.Infrastructure.Calculations;

public record TransferPackResponse
{
    public string LetterURI { get; init; }
    public string LetterURL { get; init; }
}