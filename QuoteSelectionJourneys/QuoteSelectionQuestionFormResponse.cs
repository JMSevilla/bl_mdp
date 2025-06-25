using WTW.MdpService.Domain.Common.Journeys;

namespace WTW.MdpService.QuoteSelectionJourneys;

public record QuoteSelectionQuestionFormResponse
{
    public string SelectedQuoteName { get; init; }

    public static QuoteSelectionQuestionFormResponse From(QuestionForm questionForm)
    {
        return new()
        {
            SelectedQuoteName = questionForm.AnswerKey
        };
    }
}