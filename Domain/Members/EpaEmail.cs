using System;

namespace WTW.MdpService.Domain.Members;

public class EpaEmail
{
    protected EpaEmail() { }

    public EpaEmail(Email email, DateTimeOffset utcNow, int sequenceNumber)
    {
        Email = email;
        SequenceNumber = sequenceNumber;
        EffectiveDate = utcNow;
        CreaetedAt = utcNow;
    }

    public int SequenceNumber { get; }
    public virtual Email Email { get; }
    public DateTimeOffset EffectiveDate { get; }
    public DateTimeOffset? CreaetedAt { get; }
}