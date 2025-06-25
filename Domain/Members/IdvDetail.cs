namespace WTW.MdpService.Domain.Members;

public class IdvDetail
{
    protected IdvDetail() { }

    private IdvDetail(int id, string scanResult, string documentType, int edmsNumber)
    {
        Id = id;
        ScanResult = scanResult;
        DocumentType = documentType;
        EdmsNumber = edmsNumber;
    }

    public int Id { get; }
    public string ScanResult { get; }
    public string DocumentType { get; }
    public int EdmsNumber { get; }

    public static IdvDetail Create(int id, string gbgScanResult, string documentType, int edmsNumber)
    {
        return new IdvDetail(id, GetScanResult(gbgScanResult), documentType, edmsNumber);
    }

    private static string GetScanResult(string gbgResult)
    {
        return gbgResult switch
        {
            "Pass" => "P",
            "Failed" => "F",
            "Referred" or "Refer" => "R",
            _ => string.Empty
        };
    }
}