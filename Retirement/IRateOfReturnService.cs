using System;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.Retirement;
public interface IRateOfReturnService
{
    Task<Either<Error, Option<RateOfReturnResponse>>> GetRateOfReturn(string businessGroup, string referenceNumber, DateTimeOffset startDate, DateTimeOffset effectiveDate);
}