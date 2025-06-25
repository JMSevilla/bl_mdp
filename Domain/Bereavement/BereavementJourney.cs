using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using WTW.MdpService.Domain.Common.Journeys;

namespace WTW.MdpService.Domain.Bereavement;

public class BereavementJourney : Journey
{
    public BereavementJourney() { }

    private BereavementJourney(string businessGroup,
        string referenceNumber,
        DateTimeOffset utcNow,
        int validityPeriodInMin,
        string currentPageKey,
        string nextPageKey)
        : base(businessGroup, referenceNumber, utcNow, currentPageKey, nextPageKey)
    {
        ExpirationDate = utcNow.AddMinutes(validityPeriodInMin);
    }

    public static Either<Error, BereavementJourney> Create(Guid bereavementReferenceNumber,
        string businessGroup,
        DateTimeOffset utcNow,
        string currentPageKey,
        string nextPageKey,
        int validityPeriodInMin)
    {
        if (bereavementReferenceNumber == Guid.Empty)
            return Error.New("\"BereavementReferenceNumber\" can not be default value.");

        if (string.IsNullOrWhiteSpace(businessGroup) || businessGroup.Length != 3)
            return Error.New("\"BusinessGroup\" field is required. Must be 3 characters Length.");

        if (string.IsNullOrWhiteSpace(currentPageKey) || currentPageKey.Length > 25)
            return Error.New("\"CurrentPageKey\" field is required. Up to 25  characters Length.");

        if (string.IsNullOrWhiteSpace(nextPageKey) || nextPageKey.Length > 25)
            return Error.New("\"NexttPageKey\" field is required. Up to 25  characters Length.");

        if (validityPeriodInMin < 1)
            return Error.New("\"validityPeriodInMin\" field must be greater or equel to 1.");

        return new BereavementJourney(businessGroup, bereavementReferenceNumber.ToString(), utcNow, validityPeriodInMin, currentPageKey, nextPageKey);
    }

    public bool IsExpired(DateTimeOffset utcNow)
    {
        return ExpirationDate <= utcNow;
    }

    [Obsolete("This method is not supported in BereavementJourney.")]
    public new Error? UpdateStep(string switchPageKey, string nextPageKey)
    {
        throw new NotImplementedException("This is not supported in BereavementJourney. Don't use!!");
    }

    [Obsolete("This method is not supported in BereavementJourney.")]
    public new IEnumerable<QuestionForm> QuestionForms(IEnumerable<string> questionKeys)
    {
        throw new NotImplementedException("This is not supported in BereavementJourney. Don't use!!");
    }

    public IEnumerable<QuestionForm> JourneyQuestions()
    {
        return JourneyBranches.Single(x => x.IsActive).QuestionForms();
    }

    public void UpdateExpiryDate(DateTimeOffset dateTimeOffset)
    {
        ExpirationDate = dateTimeOffset;
    }
}