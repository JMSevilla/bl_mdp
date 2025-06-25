using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.Infrastructure.Geolocation;

public interface ILoqateApiClient
{
    Task<Either<Error, LocationApiAddressSummaryResponse>> Find(string text, string container, string language, string countries);

    Task<Either<Error, LocationApiAddressDetailsResponse>> GetDetails(
        string addressId);
}