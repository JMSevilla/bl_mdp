namespace WTW.MdpService.Domain.Members;

public class EmailView
{
    protected EmailView() { }

    public EmailView(Email email)
    {
        Email = email;
    }

    public virtual Email Email { get; }
}