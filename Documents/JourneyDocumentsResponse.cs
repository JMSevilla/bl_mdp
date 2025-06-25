using System;
using System.Collections.Generic;
using System.Linq;
using WTW.MdpService.Domain.Common;
using WTW.MdpService.Domain.Common.Journeys;
using WTW.MdpService.Domain.Mdp;

namespace WTW.MdpService.Documents;

public record JourneyDocumentsResponse
{
    public JourneyDocumentsResponse(UploadedDocument document)
    {
        Filename = document.FileName;
        Tags = document.SplitTags()?.ToList();
        Uuid = document.Uuid;
        JourneyType = document.JourneyType;
        DocumentType = document.DocumentType;
    }

    public string Filename { get; init; }
    public List<string> Tags { get; init; }
    public string Uuid { get; init; }
    public string JourneyType { get; init; }
    public string DocumentType { get; init; }
}