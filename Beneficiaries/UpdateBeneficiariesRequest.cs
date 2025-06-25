using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WTW.MdpService.Beneficiaries;

public record UpdateBeneficiariesRequest
{
    public ICollection<UpdateBeneficiaryRequest> Beneficiaries { get; init; }

    [Required]
    public string ContentAccessKey { get; init; }
}

public record UpdateBeneficiaryRequest
{
    [Required]
    public string Relationship { get; init; }
    public int? Id { get; init; }
    public string Forenames { get; init; }
    public string Surname { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public string CharityName { get; init; }

    [Range(1, 9999999999)]
    public long? CharityNumber { get; init; }

    [Required, Range(0, 100)]
    public decimal? LumpSumPercentage { get; init; }
    public bool IsPensionBeneficiary { get; init; }
    
    [MaxLength(180)]
    public string Notes { get; init; }

    [Required]
    public UpdateBeneficiaryAddressRequest Address { get; init; }
}

public record UpdateBeneficiaryAddressRequest
{
    [MaxLength(25)]
    public string Line1 { get; init; }

    [MaxLength(25)]
    public string Line2 { get; init; }

    [MaxLength(25)]
    public string Line3 { get; init; }

    [MaxLength(25)]
    public string Line4 { get; init; }

    [MaxLength(25)]
    public string Line5 { get; init; }

    [MaxLength(25)]
    public string Country { get; init; }

    [MaxLength(3)]
    public string CountryCode { get; init; }

    [MaxLength(8)]
    public string PostCode { get; init; }
}