namespace WTW.MdpService.Infrastructure.MdpApi
{
    public class JwtTokenModel
    {
        public string Subject { get; set; }
        public string BusinessGroup { get; set; }
        public string ReferenceNumber { get; set; }
        public string MainBusinessGroup { get; set; }
        public string MainReferenceNumber { get; set; }
        public string BereavementReferenceNumber { get; set; }
        public string TokenId { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
