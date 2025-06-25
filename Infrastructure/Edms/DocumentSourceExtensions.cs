using System;
using WTW.MdpService.Domain.Common;

namespace WTW.MdpService.Infrastructure.Edms;

public static class DocumentSourceExtensions
{
    public static string ToDocSrcString(this DocumentSource documentSource)
    {
        return documentSource switch
        {
            DocumentSource.Outgoing => "O",
            DocumentSource.Incoming => "I",
            _ => default,
        };
    }
}