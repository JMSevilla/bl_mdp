namespace WTW.MdpService.Domain.Members;

public class LinkedMember
{
    protected LinkedMember() { }

    public LinkedMember(string referenceNumber, string businessGroup, string linkedReferenceNumber, string linkedBusinessGroup)
    {
        ReferenceNumber = referenceNumber;
        BusinessGroup = businessGroup;
        LinkedReferenceNumber = linkedReferenceNumber;
        LinkedBusinessGroup = linkedBusinessGroup;
    }

    public string ReferenceNumber { get; }
    public string BusinessGroup { get; }
    public string LinkedReferenceNumber { get; }
    public string LinkedBusinessGroup { get; }
}