using System.IO;
using System.Threading.Tasks;

namespace WTW.MdpService.Infrastructure.EmailConfirmation;

public interface IEmailConfirmationSmtpClient
{
    Task Send(string to, string from, string htmlBody, string subject);

    Task SendWithAttachment(string to, string from, string htmlBody, string subject, MemoryStream stream,
        string fileName);
}