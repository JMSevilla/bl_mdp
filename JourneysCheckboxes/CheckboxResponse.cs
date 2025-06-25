namespace WTW.MdpService.JourneysCheckboxes;

public record CheckboxResponse
{
    public CheckboxResponse(string key, bool answerValue)
    {
        Key = key;
        AnswerValue = answerValue;
    }

    public string Key { get; }
    public bool AnswerValue { get; }
}