using System;

namespace WTW.MdpService.Domain.Members;

public class ContactReference
{
    protected ContactReference() { }

    public ContactReference(Contact contact, Authorization authorization, string businessGroup, string referenceNumber, int sequenceNumber, DateTimeOffset utcNow)
    {
        BusinessGroup = businessGroup;
        ReferenceNumber = referenceNumber;
        SequenceNumber = sequenceNumber;
        StartDate = new (utcNow.Date, TimeSpan.Zero);
        Contact = contact;
        Authorization = authorization;
    }

    public string ReferenceNumber { get; }
    public string BusinessGroup { get; }
    public int SequenceNumber { get; }
    public DateTimeOffset StartDate { get; }
    public DateTimeOffset? EndDate { get; private set; }
    public virtual Contact Contact { get; }
    public virtual Authorization Authorization { get; }
    public string SchemeMemberIndicator { get; } = "M";
    public string AddressCode { get; } = "GENERAL";
    public bool? UseThisAddressForPayslips { get; } = true;
    public string Status { get; } = "A";

    public void Close(DateTimeOffset utcNow)
    {
        EndDate = new(utcNow.Date.AddDays(-1), TimeSpan.Zero);
    }
}