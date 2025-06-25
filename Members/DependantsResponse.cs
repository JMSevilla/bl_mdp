using System;
using System.Collections.Generic;
using System.Linq;
using WTW.MdpService.Domain.Members.Dependants;

namespace WTW.MdpService.Members;

public record DependantsResponse
{
    public IEnumerable<DependantResponse> Dependants { get; init; }

    public static DependantsResponse From(IEnumerable<Dependant> dependants)
    {
        return new()
        {
            Dependants = dependants.Select(x => new DependantResponse
            {
                Relationship = x.DecodedRelationship(),
                Forenames = x.Forenames,
                Surname = x.Surname,
                DateOfBirth = x.DateOfBirth?.Date,
                Address = DependantAddressResponse.From(x.Address)
            })
        };
    }
}

public record DependantResponse
{
    public string Relationship { get; init; }
    public string Forenames { get; init; }
    public string Surname { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public DependantAddressResponse Address { get; init; }
}

public record DependantAddressResponse
{
    public string Line1 { get; init; }
    public string Line2 { get; init; }
    public string Line3 { get; init; }
    public string Line4 { get; init; }
    public string Line5 { get; init; }
    public string Country { get; init; }
    public string PostCode { get; init; }

    public static DependantAddressResponse From(DependantAddress address)
    {
        return new()
        {
            Line1 = address.Line1,
            Line2 = address.Line2,
            Line3 = address.Line3,
            Line4 = address.Line4,
            Line5 = address.Line5,
            Country = address.Country,
            PostCode = address.PostCode,
        };
    }
}