using System.Collections.Generic;

namespace WTW.MdpService.Infrastructure.ApplyFinancials;

public record AccountValidationResponse
{
    public string CountryCode { get; set; }
    public string NationalId { get; set; }
    public string AccountNumber { get; set; }
    public string Status { get; set; }
    public string Comment { get; set; }
    public string RecommendedNatId { get; set; }
    public string RecommendedAcct { get; set; }
    public string RecommendedBIC { get; set; }
    public string Ref { get; set; }
    public string Group { get; set; }
    public List<BranchDetail> BranchDetails { get; set; }
    public HeadOfficeDetails HeadOfficeDetails { get; set; }
    public PaymentBicDetails PaymentBicDetails { get; set; }
    public string Bic8 { get; set; }
    public string DataStore { get; set; }
    public string NoBranch { get; set; }
    public string IsoAddr { get; set; }
    public string PayBranchType { get; set; }
    public string FreeToken { get; set; }
}

public record BranchDetail : BankDetail
{
    public CodeDetails CodeDetails { get; set; }
    public SepaDetails SepaDetails { get; set; }
    public AdditionalData AdditionalData { get; set; }
    public string BankToken { get; set; }
}

public record HeadOfficeDetails : BankDetail
{
    public CodeDetails CodeDetails { get; set; }
    public AdditionalData AdditionalData { get; set; }
}

public record PaymentBicDetails : BankDetail
{
    public string BranchTypeLabel { get; set; }
    public CodeDetails CodeDetails { get; set; }
    public AdditionalData AdditionalData { get; set; }
}

public abstract record BankDetail
{
    public string BankName { get; set; }
    public string Branch { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string PostZip { get; set; }
    public string Region { get; set; }
    public string Country { get; set; }
}

public record CodeDetails
{
    public string CodeName1 { get; set; }
    public string CodeValue1 { get; set; }
    public string CodeName2 { get; set; }
    public string CodeValue2 { get; set; }
    public string CodeName3 { get; set; }
    public string CodeValue3 { get; set; }
    public string CodeName4 { get; set; }
    public string CodeValue4 { get; set; }
}

public record SepaDetails
{
    public string CtStatus { get; set; }
    public string DdStatus { get; set; }
    public string BbStatus { get; set; }
}

public record AdditionalData
{
    public string SsiAvailable { get; set; }
    public string PayServiceAvailable { get; set; }
    public string ContactsAvailable { get; set; }
    public string MessageAvailable { get; set; }
    public string HolidayAvailable { get; set; }
}