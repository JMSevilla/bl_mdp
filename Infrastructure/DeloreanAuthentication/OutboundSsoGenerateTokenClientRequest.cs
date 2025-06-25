namespace WTW.MdpService.Infrastructure.DeloreanAuthentication;

public class OutboundSsoGenerateTokenClientRequest
{
    public OutboundClaims Claims { get; set; }
    public int Expiry { get; set; }

    public class OutboundClaims
    {
        public string ClientId { get; set; }
        public string Uid { get; set; }
        public string Bgroup { get; set; }
        public string Refno { get; set; }
        public string Application { get; set; }
        public string HasMultipleRecords { get; set; }
    }
}
