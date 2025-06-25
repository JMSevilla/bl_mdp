namespace WTW.MdpService.Domain.Mdp;

public class RetirementPostIndexEvent
{
    protected RetirementPostIndexEvent() { }

    public RetirementPostIndexEvent(
        string businessGroup,
        string referenceNumber,
        string caseNumber,
        int batchNumber,
        int retirementApplicationImageId,
        string dbId = "",
        string error = null)
    {
        BusinessGroup = businessGroup;
        ReferenceNumber = referenceNumber;
        CaseNumber = caseNumber;
        BatchNumber = batchNumber;
        RetirementApplicationImageId = retirementApplicationImageId;
        DbId = dbId;
        Error = error;
    }

    public string BusinessGroup { get; }
    public string ReferenceNumber { get; }
    public string CaseNumber { get; }
    public int BatchNumber { get; }
    public int RetirementApplicationImageId { get; }
    public string DbId { get; private set; }
    public string Error { get; private set; }

    public void SetError(string error)
    {
        Error = error;
    }
    
    public void SetDbId(string dbId)
    {
        DbId = dbId;
    }

    public override string ToString()
    {
        return $"bgroup: {BusinessGroup}, refno: {ReferenceNumber}, caseno: {CaseNumber}, batchno: {BatchNumber}";
    }
}