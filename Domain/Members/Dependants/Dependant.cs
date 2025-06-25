using System;

namespace WTW.MdpService.Domain.Members.Dependants;

public class Dependant
{
    protected Dependant() { }

    public Dependant(string referenceNumber,
        string businessGroup,
        int sequenceNumber,
        string relationshipCode,
        string forenames, string surname,
        string gender,
        DateTimeOffset? dateOfBirth,
        DateTimeOffset? utcNow,
        DependantAddress address)
    {
        ReferenceNumber = referenceNumber;
        BusinessGroup = businessGroup;
        RelationshipCode = relationshipCode;
        Forenames = forenames;
        Surname = surname;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        StartDate = utcNow;
        Address = address;
        SequenceNumber = sequenceNumber;
    }

    public string ReferenceNumber { get; }
    public string BusinessGroup { get; }
    public int SequenceNumber { get; }
    public string RelationshipCode { get; }
    public string Forenames { get; }
    public string Surname { get; }
    public string Gender { get; }
    public DateTimeOffset? DateOfBirth { get; }
    public DateTimeOffset? StartDate { get; }
    public DateTimeOffset? EndDate { get; }
    public virtual DependantAddress Address { get; }

    public string DecodedRelationship()
    {
        return (RelationshipCode, Gender) switch
        {
            ("CHLD", "F") => "Daughter",
            ("CHLD", "M") => "Son",
            ("CHLD", null) => "Child",
            ("SPOP", _) => "Spouse",
            _ => "Other Relation",
        };
    }
}