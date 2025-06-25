using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt.TypeClasses;
using Microsoft.Extensions.Options;
using WTW.MdpService.Domain;
using WTW.Web.Clients;
using WTW.Web.LanguageExt;

namespace WTW.MdpService.Infrastructure.EmailConfirmation;

public class EmailConfirmationSmtpClient : IEmailConfirmationSmtpClient
{
    private const string DefaultFromEmailAddress = "AssureSupport@mdp.wtwco.com";
    private const string DefaultFromEmailName = "Assure Support";
    private readonly IEmailClient _client;

    public EmailConfirmationSmtpClient(IEmailClient client)
    {
        _client = client;
    }

    public async Task Send(string to, string from, string htmlBody, string subject)
    {
        (var fromName, var fromEmail) = SplitFromField(from);
        await _client.Send(to, fromEmail, fromName, subject, htmlBody, default);
    }

    public async Task SendWithAttachment(string to, string from, string htmlBody, string subject, MemoryStream stream, string fileName)
    {
        (var fromName, var fromEmail) = SplitFromField(from);
        await _client.SendWithAttachement(to, fromEmail, fromName, subject, htmlBody, stream, fileName, default);
    }

    private (string name, string email) SplitFromField(string from)
    {
        var result = from?.Split(";");
        if (result == null || result.Length != 2)
            return (DefaultFromEmailName, DefaultFromEmailAddress);

        var emailOrError = Email.Create(result[1].Trim());
        if(emailOrError.IsLeft)
            return (DefaultFromEmailName, DefaultFromEmailAddress);

        return (result[0], emailOrError.Right());
    }
}