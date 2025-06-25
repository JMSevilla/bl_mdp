namespace WTW.MdpService.Beneficiaries;

public record BeneficiaryOrganizationResponse
{
    public string OrganizationType { get; set; }
    public string OrganizationName { get; set; }
    public string OrganizationReference { get; set; }
    public BeneficiaryAddressResponse Address { get; set; }
    public decimal PensionPercentage { get; set; }
    public decimal LumpSumPercentage { get; set; }
    public string NominationDate { get; set; }
    public string RevokedDate { get; set; }
    public string Remarks { get; set; }
}