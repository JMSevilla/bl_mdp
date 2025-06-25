namespace WTW.MdpService.Domain.Members;

public class EventRti
{
    protected EventRti() { }

    public EventRti(int score, string businessGroup, string caseCode, string status)
    {
        Score = score;
        BusinessGroup = businessGroup;
        CaseCode = caseCode;
        Status = status;
    }

    public int Score { get; }
    public string BusinessGroup { get; }
    public string CaseCode { get; }
    public string Status { get; }
}