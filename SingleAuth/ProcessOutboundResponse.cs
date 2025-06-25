namespace WTW.MdpService.SingleAuth;

public class ProcessOutboundResponse
{
    public string LookupCode { get; set; }
    public static ProcessOutboundResponse Create(string lookup)
    {
        return new ProcessOutboundResponse()
        {
            LookupCode = lookup
        };
    }
}
