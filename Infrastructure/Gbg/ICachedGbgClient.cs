using System;
using System.Collections.Generic;
using System.IO;
using LanguageExt;

namespace WTW.MdpService.Infrastructure.Gbg;

public interface ICachedGbgClient
{
    TryAsync<Stream> GetDocuments(ICollection<Guid> ids);
}