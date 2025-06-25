using System;

namespace WTW.MdpService.Domain.Common.Journeys;
public class QuestionForm
{
    public QuestionForm() { }

    public QuestionForm(string questionKey, string answerKey, string answerValue = null)
    {
        QuestionKey = questionKey;
        AnswerKey = answerKey;
        AnswerValue = answerValue;
    }

    public string QuestionKey { get; private set; }
    public string AnswerKey { get; private set; }
    public string AnswerValue { get; private set; }

    public QuestionForm Duplicate()
    {
        return new QuestionForm(QuestionKey, AnswerKey, AnswerValue);
    }

    public void Update(QuestionForm questionForm)
    {
        QuestionKey = questionForm?.QuestionKey;
        AnswerKey = questionForm?.AnswerKey;
        AnswerValue = questionForm?.AnswerValue;
    }
}