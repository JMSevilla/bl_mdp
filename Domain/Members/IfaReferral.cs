using System;

namespace WTW.MdpService.Domain.Members;

public class IfaReferral
{
    protected IfaReferral() { }

    public IfaReferral(string referenceNumber,
        string businessGroup,
        DateTimeOffset referralInitiatedOn,
        string referralResult,
        string calculationType)
    {
        ReferenceNumber = referenceNumber;
        BusinessGroup = businessGroup;
        ReferralResult = referralResult;
        ReferralInitiatedOn = referralInitiatedOn;
        CalculationType = calculationType;
    }

    public string ReferenceNumber { get; }
    public string BusinessGroup { get; }
    public string CalculationType { get; }
    public string ReferralResult { get; }
    public DateTimeOffset ReferralInitiatedOn { get; }
}