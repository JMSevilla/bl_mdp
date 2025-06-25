using System;

namespace WTW.MdpService.Domain.Members;

public class CalculationHistory
{
    protected CalculationHistory() { }

    public CalculationHistory(string referenceNumber, string businessGroup, string @event, int sequenceNumber, int? imageId, int? fileId)
    {
        ReferenceNumber = referenceNumber;
        BusinessGroup = businessGroup;
        Event = @event;
        SequenceNumber = sequenceNumber;
        ImageId = imageId;
        FileId = fileId;
    }

    public string ReferenceNumber { get; }
    public string BusinessGroup { get; }
    public string Event { get; }
    public int SequenceNumber { get; }
    public int? ImageId { get; private set; }
    public int? FileId { get; private set; }

    public void UpdateIds(int imageId, int? fileId)
    {
        ImageId = imageId;
        FileId = fileId;
    }
}