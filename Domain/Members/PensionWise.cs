using System;

namespace WTW.MdpService.Domain.Members;

public class PensionWise
{
    private const string PensionWiseSettlementCaseTypeRetirement = "1";

    protected PensionWise() { }

    private PensionWise(string businessGroup,
        string referenceNumber, 
        string caseNumber,
        string pensionWiseSettlementCaseType,
        DateTimeOffset? financialAdviseDate, 
        DateTimeOffset? pensionWiseDate, 
        string pwResponse,
        string reasonForExemption)
    {
        BusinessGroup = businessGroup;
        ReferenceNumber = referenceNumber;
        CaseNumber = caseNumber;
        PensionWiseSettlementCaseType = pensionWiseSettlementCaseType;
        FinancialAdviseDate = financialAdviseDate;
        PensionWiseDate = pensionWiseDate;
        PwResponse = pwResponse;
        ReasonForExemption = reasonForExemption;
    }

    public string BusinessGroup { get; }
    public string ReferenceNumber { get; }
    public int SequenceNumber { get; }
    public string CaseNumber { get; }
    public string PensionWiseSettlementCaseType { get; }
    public string PwResponse { get; }
    public string ReasonForExemption { get; }
    public DateTimeOffset? FinancialAdviseDate { get; }
    public DateTimeOffset? PensionWiseDate { get; }

    public static PensionWise Create(string businessGroup,
        string referenceNumber, 
        string caseNumber,
        DateTimeOffset? financialAdviseDate,
        DateTimeOffset? pensionWiseDate, 
        string pwAnswerKey)
    {
        var pwResponse = pwAnswerKey switch
        {
            ("pw_guidance_a") => "4",
            ("pw_guidance_b") => "1",
            ("pw_guidance_c") => "1",
            ("pw_guidance_d") => "2",
            _ => null
        };

        var reasonForExemption = pwAnswerKey switch
        {
            ("pw_guidance_b") => "C",
            ("pw_guidance_c") => "B",
            _ => null
        };

        return new PensionWise(businessGroup, referenceNumber, caseNumber, PensionWiseSettlementCaseTypeRetirement,
            financialAdviseDate, pensionWiseDate, pwResponse, reasonForExemption);
    }
}