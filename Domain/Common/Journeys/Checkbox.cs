namespace WTW.MdpService.Domain.Common.Journeys;

public class Checkbox
{
    public Checkbox(string key, bool answerValue)
    {
        Key = key;
        AnswerValue = answerValue;
    }

    public Checkbox(string key, bool answerValue, string answer)
    {
        Key = key;
        AnswerValue = answerValue;
        Answer = answer;
    }

    public string Key { get; }
    public bool AnswerValue { get; }

    public string Answer { get; }

    public Checkbox Duplicate()
    {
        return new(Key, AnswerValue);
    }
}