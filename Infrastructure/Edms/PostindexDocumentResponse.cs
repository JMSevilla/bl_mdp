namespace WTW.MdpService.Infrastructure.Edms;

public record PostindexDocumentResponse
{
    public string DocUuid { get; init; }
    public bool Indexed { get; init; }
    public string Message { get; init; }
    public int ImageId { get; init; }
}