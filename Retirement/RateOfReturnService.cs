using System;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using WTW.MdpService.Infrastructure.Calculations;

namespace WTW.MdpService.Retirement;

public class RateOfReturnService : IRateOfReturnService
{
    private readonly ICalculationsClient _calculationsClient;

    public RateOfReturnService(ICalculationsClient calculationsClient)
    {
        _calculationsClient = calculationsClient;
    }

    public async Task<Either<Error, Option<RateOfReturnResponse>>> GetRateOfReturn(string businessGroup, string referenceNumber, DateTimeOffset startDate, DateTimeOffset effectiveDate)
    {
        if (effectiveDate == default)
            effectiveDate = DateTimeOffset.UtcNow;

        if (startDate == default)
            startDate = DateTimeOffset.UtcNow.AddYears(-1);

        var retirementV2ResponseOrError = await _calculationsClient.RateOfReturn(businessGroup, referenceNumber, startDate, effectiveDate);

        return retirementV2ResponseOrError.Match<Either<Error, Option<RateOfReturnResponse>>>(
            Right: response =>
            {
                if (response.Results.Mdp.RateOfReturn.ClosingBalanceZero == "Y")
                {
                    return Option<RateOfReturnResponse>.None;
                }
                return Option<RateOfReturnResponse>.Some(new RateOfReturnResponse
                {
                    personalRateOfReturn = response.Results.Mdp.RateOfReturn.PersonalRateOfReturn,
                    changeInValue = response.Results.Mdp.RateOfReturn.ChangeInValue
                });
            },
            Left: error => error
        );
    }
}
