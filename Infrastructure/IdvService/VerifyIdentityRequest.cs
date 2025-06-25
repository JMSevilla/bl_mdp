#nullable enable
using System;

namespace WTW.MdpService.Infrastructure.IdvService;

public class VerifyIdentityRequest
{
    public VerifyIdentityRequest(Guid? journeyId, string userId, bool checkPreviousResult, string requestSource, bool overrideLivenessResult)
    {
        JourneyId = journeyId;
        UserId = userId;
        CheckPreviousResult = checkPreviousResult;
        RequestSource = requestSource;
        OverrideLivenessResult = overrideLivenessResult;
    }

    public Guid? JourneyId { get; set; }
    public string UserId { get; set; }
    public bool CheckPreviousResult { get; set; }
    public string? RequestSource { get; set; }
    public bool OverrideLivenessResult { get; set; }
}
#nullable disable
