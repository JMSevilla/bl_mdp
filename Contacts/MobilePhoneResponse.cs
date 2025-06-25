namespace WTW.MdpService.Contacts;

public record MobilePhoneResponse
{
    public string Code { get; init; }
    public string Number { get; init; }

    public static MobilePhoneResponse From(string code, string number)
    {
        return new()
        {
            Code = code,
            Number = number
        };
    }
}