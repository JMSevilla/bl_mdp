using System;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.Domain.Members;

public class ContactValidation
{
    protected ContactValidation() { }

    public ContactValidation(string userId, MemberContactType contactType, DateTimeOffset contactValidatedAt, long addressNumber, string token)
    {
        UserId = userId;
        ContactType = contactType;
        ContactValidatedAt = contactValidatedAt;
        AddressNumber = addressNumber;
        Token = Int32.Parse(token);
    }

    public string ReferenceNumber { get; }
    public string BusinessGroup { get; }
    public string UserId { get; }
    public MemberContactType ContactType { get; }
    public string ContactValid { get; } = "Y";
    public DateTimeOffset? ContactValidatedAt { get; }
    public long? AddressNumber { get; }
    public int? Token { get; }
    public string ContactPhoneType { get; }
}