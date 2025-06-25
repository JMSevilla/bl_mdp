using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using WTW.MdpService.Domain.Common.Journeys;

namespace WTW.MdpService.Domain.Common;

public class UploadedDocument
{
    protected UploadedDocument() { }

    public UploadedDocument(
        string referenceNumber,
        string businessGroup,
        string journeyType,
        string documentType,
        string fileName,
        string uuid,
        DocumentSource documentSource,
        bool isEdoc,
        params string[] tags)
    {
        ReferenceNumber = referenceNumber;
        BusinessGroup = businessGroup;
        JourneyType = journeyType;
        DocumentType = documentType;
        FileName = fileName;
        Uuid = uuid;
        DocumentSource = documentSource;
        IsEpaOnly = false;
        IsEdoc = isEdoc;
        Tags = TagsAsString(tags);
    }

    public string ReferenceNumber { get; }
    public string BusinessGroup { get; }
    public string JourneyType { get; }
    public string DocumentType { get; }
    public string FileName { get; private set; }
    public string Tags { get; private set; }
    public string Uuid { get; private set; }
    public DocumentSource? DocumentSource { get; }
    public bool IsEpaOnly { get; private set; }
    public bool IsEdoc { get; private set; }

    public UploadedDocument UpdateDocument(string uuid, IEnumerable<string> tags)
    {
        Uuid = uuid;
        Tags = TagsAsString(tags);

        return this;
    }

    public UploadedDocument UpdateTags(IEnumerable<string> tags)
    {
        Tags = TagsAsString(tags);
        return this;
    }

    public UploadedDocument UpdateFileUuidAndName(string uuid, string fileName)
    {
        FileName = fileName;
        Uuid = uuid;
        return this;
    }

    public IEnumerable<string> SplitTags()
    {
        return Tags?.Split(";");
    }

    private string TagsAsString(IEnumerable<string> tags)
    {
        return tags != null ? string.Join(";", tags.Where(x => x != null)) : null;
    }
}
