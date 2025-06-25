namespace WTW.MdpService.Beneficiaries;

public record BeneficiaryPersonResponse
{
    private string relationship;

    public string Relationship { get => char.ToUpper(relationship[0]) + relationship.Substring(1).ToLower(); init => relationship = value; }
    public string Title { get; init; }
    public string Forenames { get; init; }
    public string Surname { get; init; }
    public string Gender { get; init; }
    public string DateOfBirth { get; init; }
    public BeneficiaryAddressResponse Address { get; init; }
    public decimal? PensionPercentage { get; init; }
    public decimal? LumpSumPercentage { get; init; }
    public string NominationDate { get; init; }
    public string RevokedDate { get; init; }
    public string Remarks { get; init; }
}