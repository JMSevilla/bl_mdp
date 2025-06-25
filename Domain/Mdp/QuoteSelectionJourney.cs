using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using WTW.MdpService.Domain.Common.Journeys;

namespace WTW.MdpService.Domain.Mdp;

public class QuoteSelectionJourney : Journey
{
    public const string QuestionKey = "SelectedQuoteName";

    protected QuoteSelectionJourney() { }

    public QuoteSelectionJourney(
        string businessGroup,
        string referenceNumber,
        DateTimeOffset utcNow,
        string currentPageKey,
        string nextPageKey,
        string selectedQuoteName)
        : base(businessGroup, referenceNumber, utcNow, currentPageKey, nextPageKey, QuestionKey, selectedQuoteName)
    {
    }

    [Obsolete("This method is not supported in QuoteSelectionJourney.")]
    public new Error? UpdateStep(string switchPageKey, string nextPageKey)
    {
        throw new NotImplementedException("This is not supported in QuoteSelectionJourney. Don't use!!");
    }

    [Obsolete("This method is not supported in QuoteSelectionJourney.")]
    public new IEnumerable<QuestionForm> QuestionForms(IEnumerable<string> questionKeys)
    {
        throw new NotImplementedException("This is not supported in QuoteSelectionJourney. Don't use!!");
    }

    public Option<string> QuoteSelection()
    {
        return JourneyBranches
            .Single(x => x.IsActive)
            .QuestionForms()
            .OrderByDescending(x => x.AnswerKey.Length)
            .FirstOrDefault()
            ?.AnswerKey;
    }
}