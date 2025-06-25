using System.Net;
using LanguageExt;

namespace WTW.MdpService.Infrastructure.Gbg;

public interface ICachedGbgAdminClient
{
    TryAsync<HttpStatusCode> DeleteJourneyPerson(string gbgId);
}