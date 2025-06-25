using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using WTW.Web.Attributes;

namespace WTW.MdpService.BereavementJourneys;

[EscapeHtmlProperties]
public record BereavementJourneySubmitRequest
{
    [Required]
    public string TenantUrl { get; init; }

    [Required]
    public BereavementJourneyDeceasedPerson Deceased { get; init; }  
        
    [Required]
    public BereavementJourneyPerson Reporter { get; init; }
    public BereavementJourneyPerson NextOfKin { get; init; }
    public BereavementJourneyPerson Executor { get; init; }
    public BereavementJourneyPerson ContactPerson { get; init; }

    public record BereavementJourneyPerson
    {
        public string Name { get; init; }
        public string Surname { get; init; }
        public string Relationship { get; init; }
        public string Email { get; init; }
        [MaxLength(5)]
        public string PhoneCode { get; init; }
        [MaxLength(20)]
        public string PhoneNumber { get; init; }
        public BereavementJourneyAddressRequest Address { get; init; }
    }

    public record BereavementJourneyDeceasedPerson
    {
        public string Name { get; init; }
        public string Surname { get; init; }
        public DateTime? DateOfBirth { get; init; }
        public DateTime? DateOfDeath { get; init; }
        public BereavementJourneyAddressRequest Address { get; init; }
        public DeceasedPersonIdentification Identification { get; init; }
    }

    public record DeceasedPersonIdentification
    {
        public string Type { get; init; }
        public string NationalInsuranceNumber { get; init; }
        public string PersonalPublicServiceNumber { get; init; }
        public List<string> PensionReferenceNumbers { get; init; }
    }

    public record BereavementJourneyAddressRequest
    {
        public string Line1 { get; init; }
        public string Line2 { get; init; }
        public string Line3 { get; init; }
        public string Line4 { get; init; }
        public string Line5 { get; init; }
        public string Country { get; init; }
        public string CountryCode { get; init; }
        public string PostCode { get; init; }
    }
}