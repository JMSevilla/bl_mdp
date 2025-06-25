using System;

namespace WTW.MdpService.Domain.Members;

public class Authorization
{
    protected Authorization() { }

    public Authorization(string businessGroup, string referenceNumber, long authorizationNumber, DateTimeOffset utcNow)
    {
        BusinessGroup = businessGroup;
        ReferenceNumber = referenceNumber;
        AuthorizationNumber = authorizationNumber;
        AcitivityCarriedOutDate = utcNow;
        AcitivityAuthorizedDate = utcNow;
        AcitivityProcessedDate = utcNow;
    }

    public string ReferenceNumber { get; }
    public string BusinessGroup { get; }
    public long AuthorizationNumber { get; }
    public string AuthorizationCode { get; } = "CIARD";
    public string UserWhoCarriedOutActivity { get; } = "MDP";
    public string UserWhoAuthorisedActivity { get; } = "MDP";
    public string SchemeMemberIndicator { get; } = "M";
    public DateTimeOffset AcitivityCarriedOutDate { get; }
    public DateTimeOffset? AcitivityAuthorizedDate { get; }
    public DateTimeOffset? AcitivityProcessedDate { get; }
}