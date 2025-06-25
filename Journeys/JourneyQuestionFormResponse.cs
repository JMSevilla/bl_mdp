using WTW.MdpService.Domain.Common.Journeys;

namespace WTW.MdpService.Journeys;

public record JourneyQuestionFormResponse
{
    public JourneyQuestionFormResponse(QuestionForm questionForm)
    {
        QuestionKey = questionForm.QuestionKey;
        AnswerKey = questionForm.AnswerKey;
        AnswerValue = questionForm.AnswerValue;
    }

    public string QuestionKey { get; init; }
    public string AnswerKey { get; init; }
    public string AnswerValue { get; init; }
}