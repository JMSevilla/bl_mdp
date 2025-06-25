using System;
using System.Collections.Generic;
using System.Linq;
using WTW.MdpService.Domain.Members.Beneficiaries;

namespace WTW.MdpService.Beneficiaries;

public record BeneficiariesResponse
{
    public IEnumerable<BeneficiaryResponse> Beneficiaries { get; init; }

    public static BeneficiariesResponse From(IEnumerable<Beneficiary> beneficiaries)
    {
        return new()
        {
            Beneficiaries = beneficiaries.Select(x => new BeneficiaryResponse
            {
                Id = x.SequenceNumber,
                Relationship = x.BeneficiaryDetails.Relationship,
                Forenames = x.BeneficiaryDetails.Forenames,
                Surname = x.IsCharity() ? null : x.BeneficiaryDetails.MixedCaseSurname,
                DateOfBirth = x.BeneficiaryDetails.DateOfBirth?.Date,
                CharityName = x.BeneficiaryDetails.CharityName,
                CharityNumber = x.BeneficiaryDetails.CharityNumber,
                LumpSumPercentage = x.BeneficiaryDetails.LumpSumPercentage,
                IsPensionBeneficiary = x.IsPensionBeneficiary(),
                Notes = x.BeneficiaryDetails.Notes,
                Address = BeneficiaryAddressResponse.From(x.Address)
            })
        };
    }
}

public record BeneficiaryResponse
{
    public int Id { get; init; }
    public string Relationship { get; init; }
    public string Forenames { get; init; }
    public string Surname { get; init; }
    public string CharityName { get; init; }
    public decimal? LumpSumPercentage { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public long? CharityNumber { get; init; }
    public bool IsPensionBeneficiary { get; init; }
    public string Notes { get; init; }
    public BeneficiaryAddressResponse Address { get; init; }
}

public record BeneficiaryAddressResponse
{
    public string Line1 { get; init; }
    public string Line2 { get; init; }
    public string Line3 { get; init; }
    public string Line4 { get; init; }
    public string Line5 { get; init; }
    public string Country { get; init; }
    public string CountryCode { get; init; }
    public string PostCode { get; init; }

    public static BeneficiaryAddressResponse From(BeneficiaryAddress address)
    {
        return new()
        {
            Line1 = address.Line1,
            Line2 = address.Line2,
            Line3 = address.Line3,
            Line4 = address.Line4,
            Line5 = address.Line5,
            Country = address.Country,
            CountryCode = address.CountryCode,
            PostCode = address.PostCode,
        };
    }
}