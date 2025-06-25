using WTW.MdpService.Domain.Common.Journeys;

namespace WTW.MdpService.TransferJourneys;

public class TransferQuestionFormResponse
{
    public string QuestionKey { get; init; }
    public string AnswerKey { get; init; }
    public string AnswerValue { get; init; }

    public static TransferQuestionFormResponse From(QuestionForm questionForm)
    {
        return new()
        {
            QuestionKey = questionForm.QuestionKey,
            AnswerKey = questionForm.AnswerKey,
            AnswerValue = questionForm.AnswerValue
        };
    }
}