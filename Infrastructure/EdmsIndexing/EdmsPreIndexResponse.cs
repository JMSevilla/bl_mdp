namespace WTW.MdpService.Infrastructure.EdmsIndexing;

public class EdmsPreIndexResult
{
    public EdmsPreIndexResult(int batchNumber, int applicationImageId)
    {
        BatchNumber = batchNumber;
        ApplicationImageId = applicationImageId;
    }

    public int BatchNumber { get; }
    public int ApplicationImageId { get; }
}