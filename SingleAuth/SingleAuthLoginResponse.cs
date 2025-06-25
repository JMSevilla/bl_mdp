using System.Collections.Generic;

namespace WTW.MdpService.SingleAuth;

public class SingleAuthLoginResponse
{
    public string BusinessGroup { get; set; }
    public string ReferenceNumber { get; set; }
    public bool HasMultipleRecords { get; set; }
    public List<string> EligibleRecords { get; set; }
}
