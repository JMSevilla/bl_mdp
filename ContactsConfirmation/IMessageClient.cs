using LanguageExt;
using LanguageExt.Common;

namespace WTW.MdpService.ContactsConfirmation;

public interface IMessageClient
{
    Either<Error, string> SendMessage(string originator, string body, string[] recipients);
}