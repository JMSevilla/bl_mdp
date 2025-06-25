using System;
using WTW.Web;

namespace WTW.MdpService.Domain.Members;

public class PaperRetirementApplication
{
    protected PaperRetirementApplication() { }

    public PaperRetirementApplication(string code,
        string caseCode,
        string caseNumber,
        string eventType,
        string status,
        DateTimeOffset? caseCompletionDate,
        DateTimeOffset? caseReceivedDate,
        BatchCreateDetails batchCreateDeatils)
    {
        Code = code;
        CaseCode = caseCode;
        CaseNumber = caseNumber;
        EventType = eventType;
        Status = status;
        CaseCompletionDate = caseCompletionDate;
        CaseReceivedDate = caseReceivedDate;
        BatchCreateDeatils = batchCreateDeatils;
    }

    public string Code { get; }
    public string CaseCode { get; }
    public string CaseNumber { get; }
    public string EventType { get; }
    public string Status { get; }
    public DateTimeOffset? CaseReceivedDate { get; }
    public DateTimeOffset? CaseCompletionDate { get; }
    public virtual BatchCreateDetails BatchCreateDeatils { get; }

    public bool IsPaperRetirementApplicationSubmitted()
    {
        return (CaseCode == "RTP9" || CaseCode == "TOP9") && string.IsNullOrWhiteSpace(Code) && BatchCreateDeatils != null && BatchCreateDeatils.IsPaperRetirementApplicationSubmitted();
    }

    public bool IsTransferRetirementApplicationSubmitted()
    {
        return string.IsNullOrWhiteSpace(Code) && CaseCode == "TOP9";
    }

    public bool IsClosedOrAbandoned()
    {
        return !string.IsNullOrEmpty(Code) && CaseCompletionDate.HasValue;
    }

    public bool IsAbandoned()
    {
        return string.Equals(Code, "ac", StringComparison.OrdinalIgnoreCase);
    }

    public bool IsRTP9()
    {
        return CaseCode == "RTP9";
    }

    public bool IsDeathCasesLogged() => (CaseCode == MdpConstants.DeathCaseCodes.DDD9 || CaseCode == MdpConstants.DeathCaseCodes.DDR9 || CaseCode == MdpConstants.DeathCaseCodes.DDA9) && (Code ?? "ZZ") != "AC";
}