namespace WTW.MdpService.Domain.Members;

public class UserQueryPrompt
{
    protected UserQueryPrompt() { }

    public UserQueryPrompt(int scoreNumber, string businessGroup, string caseCode, string status, string @event)
    {
        ScoreNumber = scoreNumber;
        BusinessGroup = businessGroup;
        CaseCode = caseCode;
        Status = status;
        Event = @event;
    }

    public int ScoreNumber { get; }
    public string BusinessGroup { get; }
    public string CaseCode { get; }
    public string Status { get; }
    public string Event { get; }
}