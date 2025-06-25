namespace WTW.MdpService.Infrastructure.IdvService;

public class SaveIdentityVerificationRequest
{
    public SaveIdentityVerificationRequest(string caseCode, string caseNo)
    {
        CaseCode = caseCode;
        CaseNo = caseNo;
    }

    public string CaseCode { get; set; }
    public string CaseNo { get; set; }
}
