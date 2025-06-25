namespace WTW.MdpService.Infrastructure.MemberService;

public class MemberContactDetailsClientResponse
{
    public string Telephone { get; set; }
    public string Mobile { get; set; }
    public string Email { get; set; }
    public MemberContactDetailsAddressResponse Address { get; set; }
    public string NonStandardCommsType { get; set; }

}

public class MemberContactDetailsAddressResponse
{
    public string Line1 { get; set; }
    public string Line2 { get; set; }
    public string Line3 { get; set; }
    public string Line4 { get; set; }
    public string Line5 { get; set; }
    public string PostCode { get; set; }
    public string Country { get; set; }
    public string CountryCode { get; set; }
    public string IsoCountryCode { get; set; }
}
