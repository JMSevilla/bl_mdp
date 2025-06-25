namespace WTW.MdpService.Domain.Members;

public class SdDomainList
{
    protected SdDomainList() { }

    public SdDomainList(string businessGroup, string domain, string listValue)
    {
        BusinessGroup = businessGroup;
        Domain = domain;
        ListValue = listValue;
    }
    public SdDomainList(string businessGroup, string domain, string listValue, string listDescription)
    {
        BusinessGroup = businessGroup;
        Domain = domain;
        ListValue = listValue;
        ListValueDescription = listDescription;
    }
    public SdDomainList(string businessGroup, string domain, string listValue, string listDescription, string sysValue)
    {
        BusinessGroup = businessGroup;
        Domain = domain;
        ListValue = listValue;
        ListValueDescription = listDescription;
        SystemValue = sysValue;
    }

    public string BusinessGroup { get; }
    public string Domain { get; }
    public string ListValue { get; }
    public string ListValueDescription { get; }
    public string SystemValue { get; }
}