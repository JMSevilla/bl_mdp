namespace WTW.MdpService.Contacts;

public record EmailResponse
{
    public string Email { get; init; }

    public static EmailResponse From(string email)
    {
        return new()
        {
            Email = email
        };
    }
}