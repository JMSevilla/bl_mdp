using WTW.MdpService.Domain.Members;

namespace WTW.MdpService.TransferJourneys;

public record TransferApplicationStatusResponse
{
    public TransferApplicationStatus Status { get; init; }

    public static TransferApplicationStatusResponse From(TransferApplicationStatus status)
    {
        return new()
        {
            Status = status
        };
    }
}