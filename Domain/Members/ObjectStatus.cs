namespace WTW.MdpService.Domain.Members;

public class ObjectStatus
{
    protected ObjectStatus() { }

    public ObjectStatus(string businessGroup, string objectId, string statusAccess, string tableShort)
    {
        BusinessGroup = businessGroup;
        ObjectId = objectId;
        StatusAccess = statusAccess;
        TableShort = tableShort;
    }

    public string BusinessGroup { get; }
    public string ObjectId { get; }
    public string StatusAccess { get; }
    public string TableShort { get; }
}