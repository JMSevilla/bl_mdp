namespace WTW.MdpService.SingleAuth.Services;

public class BgroupRefnoData
{
    public BgroupRefnoData(string bgroup,string refno,string mainBgroup,string mainRefno)
    {
        this.Bgroup= bgroup;
        this.ReferenceNumber = refno;
        this.MainBgroup = mainBgroup;
        this.MainReferenceNumber = mainRefno;
    }
    public string Bgroup { get; set; }
    public string MainBgroup { get; set; }
    public string ReferenceNumber { get; set; }
    public string MainReferenceNumber { get; set; }
}
