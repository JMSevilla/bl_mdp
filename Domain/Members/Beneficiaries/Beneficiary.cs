using System;
using System.Collections.Generic;

namespace WTW.MdpService.Domain.Members.Beneficiaries;

public class Beneficiary
{
    protected Beneficiary() { }

    public Beneficiary(
        int sequenceNumber,
        BeneficiaryAddress address,
        BeneficiaryDetails beneficiaryDetails,
        DateTimeOffset nominationDate)
    {
        SequenceNumber = sequenceNumber;
        NominationDate = nominationDate;

        Address = address;
        BeneficiaryDetails = beneficiaryDetails;
    }

    public string ReferenceNumber { get; }
    public string BusinessGroup { get; }
    public int SequenceNumber { get; }
    public virtual BeneficiaryAddress Address { get; }
    public virtual BeneficiaryDetails BeneficiaryDetails { get; }
    public DateTimeOffset? RevokeDate { get; private set; }
    public DateTimeOffset? NominationDate { get; }

    public void Revoke(DateTimeOffset utcNow)
    {
        RevokeDate = utcNow;
    }

    public bool IsCharity()
    {
        return BeneficiaryDetails.Relationship == BeneficiaryDetails.CharityStatus;
    }

    public bool IsPensionBeneficiary()
    {
        return BeneficiaryDetails.PensionPercentage == 100;
    }
}