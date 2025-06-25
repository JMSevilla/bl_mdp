namespace WTW.MdpService.Infrastructure.Investment.AnnuityBroker;

public class InvestmentQuoteMemberSummary
{
    public string NiNumber { get; set; }
    public string PayrollNo { get; set; }
    public string Title { get; set; }
    public string Surname { get; set; }
    public string Forenames { get; set; }
    public string DateOfBirth { get; set; }
    public string Sex { get; set; }
    public InvestmentQuoteMemberAddress Address { get; set; }
    public string Telephone { get; set; }
    public string Email { get; set; }
    public string MaritalStatus { get; set; }
    public string MemberStatus { get; set; }
    public string NonStandardCommsType { get; set; }
    public string SchemeName { get; set; }
    public string SchemeType { get; set; }
    public string SchemeCode { get; set; }
    public string BusinessGroupTitle { get; set; }
}