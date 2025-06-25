using System;

namespace WTW.MdpService.Domain.Members;

public class Document
{
    protected Document() { }

    public Document(
        string businessGroup,
        string referenceNumber,
        string type,
        DateTimeOffset date,
        string name,
        string fileName,
        int id,
        int imageId,
        string typeId,
        string schema)
    {
        BusinessGroup = businessGroup;
        ReferenceNumber = referenceNumber;
        Type = type;
        Date = date;
        Name = name;
        Id = id;
        FileName = fileName;
        ImageId = imageId;
        TypeId = typeId;
        Schema = schema;
    }

    public string ReferenceNumber { get; }
    public string BusinessGroup { get; }
    public string Type { get; }
    public DateTimeOffset Date { get; }
    public DateTimeOffset? LastReadDate { get; private set; }
    public string Name { get; }
    public string FileName { get; }
    public int ImageId { get; }
    public string TypeId { get; }
    public int Id { get; }
    public string Schema { get; }

    public void MarkAsRead(DateTimeOffset utcNow)
    {
        LastReadDate = utcNow;
    }
}