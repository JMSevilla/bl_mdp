using System;

namespace WTW.MdpService.Domain.Members;

public class IfaReferralHistory
{
    protected IfaReferralHistory() { }

    public IfaReferralHistory(string referenceNumber, 
        string businessGroup, 
        int sequenceNumber,
        ReferralStatus referralStatus,
        string referralResult, 
        DateTimeOffset referralInitiatedOn,
        DateTimeOffset? referralStatusDate)
    {
        ReferenceNumber = referenceNumber;
        BusinessGroup = businessGroup;
        SequenceNumber = sequenceNumber;
        ReferralStatus = referralStatus;
        ReferralResult = referralResult;
        ReferralInitiatedOn = referralInitiatedOn;
        ReferralStatusDate = referralStatusDate;
    }

    public string ReferenceNumber { get; }
    public string BusinessGroup { get; }
    public int SequenceNumber { get; }
    public ReferralStatus ReferralStatus { get; }
    public string ReferralResult { get; }
    public DateTimeOffset ReferralInitiatedOn { get; }
    public DateTimeOffset? ReferralStatusDate { get; }
}